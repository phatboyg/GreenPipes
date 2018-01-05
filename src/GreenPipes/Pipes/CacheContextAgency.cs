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
    using System.Threading;
    using System.Threading.Tasks;


    /// <summary>
    /// Maintains a cached context, which is created upon first use, and recreated whenever a fault is propogated to the
    /// usage.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class CacheContextAgency<TContext> :
        Agency,
        ICacheContextAgency<TContext>
        where TContext : class, PipeContext
    {
        readonly IAgentContextFactory<TContext> _agentContextFactory;
        readonly object _contextLock = new object();
        AgentContextHandle<TContext> _context;

        /// <summary>
        /// Create the cache
        /// </summary>
        /// <param name="agentContextFactory">Factory used to create the underlying and active contexts</param>
        public CacheContextAgency(IAgentContextFactory<TContext> agentContextFactory)
        {
            _agentContextFactory = agentContextFactory;
        }

        Task<ActiveAgentContextHandle<TContext>> IAgency<TContext>.GetActiveContext(CancellationToken cancellationToken)
        {
            return CreateActiveContext(cancellationToken);
        }

        async Task ICacheContextAgency<TContext>.Send(IPipe<TContext> pipe, CancellationToken cancellationToken)
        {
            var activeContext = await CreateActiveContext(cancellationToken).ConfigureAwait(false);

            IContextAgentProvocateur<TContext> contextAgent = new ContextAgentProvocateur<TContext>(activeContext);

            Add(contextAgent);

            try
            {
                TContext pipeContext = await activeContext.Context.ConfigureAwait(false);

                await pipe.Send(pipeContext).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await activeContext.Faulted(exception).ConfigureAwait(false);

                throw;
            }
            finally
            {
                await contextAgent.Stop(cancellationToken).ConfigureAwait(false);
            }
        }

        Task<ActiveAgentContextHandle<TContext>> CreateActiveContext(CancellationToken cancellationToken)
        {
            AgentContextHandle<TContext> context;

            lock (_contextLock)
            {
                context = _context;
                if (context == null || context.IsInactive)
                {
                    context = _agentContextFactory.CreateContext(this);

                    _context = context;

                    void ClearContext(Task task)
                    {
                        Interlocked.CompareExchange(ref _context, null, context);
                    }

                    context.Context.ContinueWith(ClearContext, TaskContinuationOptions.NotOnRanToCompletion);
                }
            }

            return _agentContextFactory.CreateActiveContext(context, cancellationToken);
        }
    }
}