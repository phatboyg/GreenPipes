// Copyright 2012-2018 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace GreenPipes.Agents
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Internals.Extensions;
    using Util;


    /// <summary>
    /// Supervises a set of agents, allowing for graceful Start, Stop, and Ready state management
    /// </summary>
    public class Supervisor :
        Agent,
        ISupervisor
    {
        readonly Dictionary<long, IAgent> _agents;
        long _nextId;

        /// <summary>
        /// Creates a Supervisor
        /// </summary>
        public Supervisor()
        {
            _agents = new Dictionary<long, IAgent>();
        }

        /// <inheritdoc />
        public void Add(IAgent agent)
        {
            if (IsStopping)
                throw new OperationCanceledException("The agent is stopped or has been stopped, no additional provocateurs can be created.");

            var id = Interlocked.Increment(ref _nextId);
            lock (_agents)
            {
                _agents.Add(id, agent);
            }

            KeyValuePair<long, IAgent>[] agents;
            lock (_agents)
            {
                agents = _agents.ToArray();
            }

            SetReady(WhenAll(agents, nameof(Ready), x => x.Ready));

            void RemoveAgent(Task task)
            {
                Remove(id);
            }

            agent.Completed.ContinueWith(RemoveAgent, TaskScheduler.Default);
        }

        /// <inheritdoc />
        public override void SetReady()
        {
            KeyValuePair<long, IAgent>[] agents;
            lock (_agents)
            {
                if (_agents.Count == 0)
                {
                    SetReady(TaskUtil.Completed);
                    return;
                }

                agents = _agents.ToArray();
            }

            SetReady(WhenAll(agents, nameof(Ready), x => x.Ready));
        }

        /// <inheritdoc />
        protected override Task StopAgent(StopContext context)
        {
            KeyValuePair<long, IAgent>[] agents;
            lock (_agents)
            {
                if (_agents.Count == 0 || (agents = _agents.Where(x => !x.Value.Completed.IsCompleted).ToArray()).Length == 0)
                {
                    SetCompleted(TaskUtil.Completed);
                    return TaskUtil.Completed;
                }
            }

            SetCompleted(WhenAll(agents, nameof(Completed), x => x.Completed));

            return Task.WhenAll(agents.Select(x => x.Value.Stop(context)).ToArray())
                .UntilCompletedOrCanceled(context.CancellationToken);
        }

        void Remove(long id)
        {
            lock (_agents)
            {
                _agents.Remove(id);
            }
        }

        Task WhenAll(KeyValuePair<long, IAgent>[] agents, string readyOrCompleted, Func<IAgent, Task> selector)
        {
            if (Trace.Listeners.Count == 0)
                return Task.WhenAll(agents.Select(x => selector(x.Value)).ToArray());

            async Task WaitForAll()
            {
                await Task.Yield();

                List<Task> faultedTasks = new List<Task>();
                do
                {
                    var delayTask = Task.Delay(1000);

                    var readyTask = await Task.WhenAny(agents.Select(x => selector(x.Value)).Concat(Enumerable.Repeat(delayTask, 1))).ConfigureAwait(false);
                    if (delayTask == readyTask)
                    {
                        Trace.WriteLine($"Waiting: {ToString()}");
                        Trace.WriteLine(string.Join(Environment.NewLine, agents.Select(agent => $"{agent} - {selector(agent.Value).Status}")));
                    }
                    else
                    {
                        Trace.WriteLine($"{readyOrCompleted} Updated: {ToString()}");
                        var completed = from agent in agents
                            let task = selector(agent.Value)
                            where task.IsCompleted
                            select new {agent, task};

                        var completedAgents = completed.ToDictionary(x => x.agent);

                        foreach (var item in completedAgents.Values)
                            if (item.task.IsCanceled)
                                Trace.WriteLine($"Canceled: {item.agent}");
                            else if (item.task.IsFaulted)
                            {
                                Trace.WriteLine($"Faulted: {item.agent}");
                                faultedTasks.Add(item.task);
                            }
                            else
                                Trace.WriteLine($"{readyOrCompleted}: {item.agent}");

                        agents = agents.Where(x => !completedAgents.ContainsKey(x)).ToArray();
                    }
                }
                while (agents.Length > 0);

                if (faultedTasks.Count > 0)
                    await Task.WhenAll(faultedTasks).ConfigureAwait(false);
            }

            return WaitForAll();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Supervisor";
        }
    }
}