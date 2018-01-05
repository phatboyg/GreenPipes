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

        public Agency()
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

            SetReady(Task.WhenAll(_agents.Values.Select(x => x.Ready).ToArray()));
            SetCompleted(Task.WhenAll(_agents.Values.Select(x => x.Completed).ToArray()));

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
            if (agents.Count == 0)
            {
                SetReady(TaskUtil.Completed);
            }
            else
            {
                SetReady(Task.WhenAll(agents.Select(x => x.Ready).ToArray()));
                SetCompleted(Task.WhenAll(agents.Select(x => x.Completed).ToArray()));
            }
        }

        /// <inheritdoc />
        protected override Task StopAgent(StopContext context)
        {
            ICollection<IAgent> agents = _agents.Values;
            if (agents.Count == 0)
            {
                SetCompleted(TaskUtil.Completed);

                return TaskUtil.Completed;
            }

            return Task.WhenAll(agents.Select(x => x.Stop(context)).ToArray());
        }

        void Remove(long id)
        {
            _agents.TryRemove(id, out var _);

            SetReady(Task.WhenAll(_agents.Values.Select(x => x.Ready).ToArray()));
            SetCompleted(Task.WhenAll(_agents.Values.Select(x => x.Completed).ToArray()));
        }
    }
}