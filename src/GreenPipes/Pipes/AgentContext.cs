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
    using System.Threading.Tasks;
    using Agents;


    /// <summary>
    /// An AgentContext
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class AgentContext<TContext> :
        AgentProvocateur,
        AgentContextHandle<TContext>
        where TContext : class, PipeContext
    {
        readonly Task<TContext> _context;
        readonly TaskCompletionSource<DateTime> _inactive;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public AgentContext(TContext context)
        {
            _context = Task.FromResult(context);
            _inactive = new TaskCompletionSource<DateTime>();

            SetReady(_context);
        }

        bool AgentContextHandle<TContext>.IsInactive => _inactive.Task.Status != TaskStatus.WaitingForActivation;

        Task<TContext> AgentContextHandle<TContext>.Context => _context;

        /// <inheritdoc />
        public async Task Disavow()
        {
            _inactive.TrySetResult(DateTime.UtcNow);

            if (_context.Status == TaskStatus.RanToCompletion)
                if (_context.Result is IAsyncDisposable disposable)
                    await disposable.DisposeAsync().ConfigureAwait(false);

            SetCompleted(_inactive.Task);
        }

        /// <inheritdoc />
        protected override Task StopAgent(StopContext context)
        {
            return Disavow();
        }
    }
}