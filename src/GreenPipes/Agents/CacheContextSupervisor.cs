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
    using System.Threading;
    using System.Threading.Tasks;
    using Internals.Extensions;


    /// <summary>
    /// Maintains a cached context, which is created upon first use, and recreated whenever a fault is propogated to the
    /// usage.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class CacheContextSupervisor<TContext> :
        Supervisor,
        ICacheContextSupervisor<TContext>
        where TContext : class, PipeContext
    {
        static readonly string Caption = $"Cache<{TypeCache<TContext>.ShortName}>";

        readonly IPipeContextFactory<TContext> _contextFactory;
        readonly object _contextLock = new object();
        PipeContextHandle<TContext> _context;

        /// <summary>
        /// Create the cache
        /// </summary>
        /// <param name="contextFactory">Factory used to create the underlying and active contexts</param>
        public CacheContextSupervisor(IPipeContextFactory<TContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        async Task ISource<TContext>.Send(IPipe<TContext> pipe, CancellationToken cancellationToken)
        {
            var activeContext = CreateActiveContext(cancellationToken);

            try
            {
                TContext context = await activeContext.Context.ConfigureAwait(false);

                await pipe.Send(context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await activeContext.Faulted(exception).ConfigureAwait(false);

                throw;
            }
            finally
            {
                await activeContext.Stop(cancellationToken).ConfigureAwait(false);
            }
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateScope("source");
            scope.Add("type", Caption);
        }

        IActivePipeContextAgent<TContext> CreateActiveContext(CancellationToken cancellationToken)
        {
            PipeContextHandle<TContext> pipeContextHandle = GetContext();

            var activeContext = _contextFactory.CreateActiveContext(this, pipeContextHandle, cancellationToken);

            var contextAgent = new ActivePipeContextAgent<TContext>(activeContext);

            Add(contextAgent);

            return contextAgent;
        }

        PipeContextHandle<TContext> GetContext()
        {
            lock (_contextLock)
            {
                if (_context != null && _context.IsDisposed == false)
                    return _context;

                PipeContextHandle<TContext> context = _context = _contextFactory.CreateContext(this);

                void ClearContext(Task task)
                {
                    Interlocked.CompareExchange(ref _context, null, context);
                }

                context.Context.ContinueWith(ClearContext, CancellationToken.None, TaskContinuationOptions.NotOnRanToCompletion, TaskScheduler.Default);

                return context;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Caption;
        }
    }
}