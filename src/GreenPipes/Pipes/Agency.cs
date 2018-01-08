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
namespace GreenPipes.Pipes
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Agents;
    using Util;


    public class Agency :
        AgentProvocateur,
        IAgency
    {
        readonly ConcurrentDictionary<long, IAgent> _agents;
        long _nextId;

        public Agency(string caption)
            : base(caption)
        {
            _agents = new ConcurrentDictionary<long, IAgent>();
        }

        /// <inheritdoc />
        public void Add(IAgent agent)
        {
            if (IsStopping)
                throw new OperationCanceledException("The agent is stopped or has been stopped, no additional provocateurs can be created.");

            var id = Interlocked.Increment(ref _nextId);

            while (_agents.TryAdd(id, agent) == false)
                id = Interlocked.Increment(ref _nextId);

            SetReady(WhenAll(_agents.ToArray(), x => x.Ready));

            void RemoveAgent(Task task)
            {
                Remove(id);
            }

            agent.Completed.ContinueWith(RemoveAgent, TaskScheduler.Default);
            agent.Ready.ContinueWith(RemoveAgent, CancellationToken.None, TaskContinuationOptions.NotOnRanToCompletion, TaskScheduler.Default);
        }

        /// <summary>
        /// Set the agency as ready
        /// </summary>
        public void SetReady()
        {
            ICollection<IAgent> agents = _agents.Values;
            if (_agents.Count == 0)
            {
                SetReady(TaskUtil.Completed);
            }
            else
            {
                SetReady(WhenAll(_agents.ToArray(), x => x.Ready));
            }
        }

        /// <inheritdoc />
        protected override Task StopAgent(StopContext context)
        {
            if (_agents.Count == 0)
            {
                SetCompleted(TaskUtil.Completed);

                return TaskUtil.Completed;
            }

            KeyValuePair<long, IAgent>[] agents = _agents.ToArray();

            SetCompleted(WhenAll(agents, x => x.Completed));

            return Task.WhenAll(_agents.Select(x => x.Value.Stop(context)).ToArray());
        }

        void Remove(long id)
        {
            _agents.TryRemove(id, out var _);
        }

        async Task WhenAll(KeyValuePair<long, IAgent>[] agents, Func<IAgent, Task> selector)
        {
            do
            {
                var delayTask = Task.Delay(1000);

                var readyTask = await Task.WhenAny(agents.Select(x => selector(x.Value)).Concat(Enumerable.Repeat(delayTask, 1))).ConfigureAwait(false);
                if (delayTask == readyTask)
                {
                    if (Trace.Listeners.Count > 0)
                    {
                        Trace.WriteLine($"Waiting: {ToString()}");
                        foreach (var task in agents)
                        {
                            Trace.WriteLine($"{task} - {selector(task.Value).Status}");
                        }
                    }
                }
                else
                {
                    if (Trace.Listeners.Count > 0)
                    {
                        IEnumerable<KeyValuePair<long, IAgent>> ready = agents
                            .Where(x => selector(x.Value).Status == TaskStatus.RanToCompletion || selector(x.Value).Status == TaskStatus.Canceled);

                        foreach (var agent in ready)
                        {
                            Trace.WriteLine($"Completed: {agent} - {ToString()}");
                        }
                    }

                    agents = agents
                        .Where(x => selector(x.Value).Status != TaskStatus.RanToCompletion && selector(x.Value).Status != TaskStatus.Canceled)
                        .ToArray();
                }
            }
            while (agents.Length > 0);
        }
    }
}