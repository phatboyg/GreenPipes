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
    using System.Threading;
    using System.Threading.Tasks;
    using Agents;


    /// <summary>
    /// An Agent Provocateur that uses a context handle for the activate state of the agent
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class ContextAgentProvocateur<TContext> :
        AgentProvocateur,
        IContextAgentProvocateur<TContext>
        where TContext : class, PipeContext
    {
        readonly AgentContextHandle<TContext> _context;

        public ContextAgentProvocateur(AgentContextHandle<TContext> context)
        {
            _context = context;

            context.Context.ContinueWith(SetReady, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
            context.Context.ContinueWith(SetFaulted, CancellationToken.None, TaskContinuationOptions.NotOnRanToCompletion, TaskScheduler.Default);
        }

        /// <inheritdoc />
        protected override async Task StopAgent(StopContext context)
        {
            if (_context.Context.Status == TaskStatus.RanToCompletion)
                await _context.Disavow().ConfigureAwait(false);
        }
    }
}