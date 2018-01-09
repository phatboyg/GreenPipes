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
namespace GreenPipes.Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Filters;
    using GreenPipes.Agents;
    using NUnit.Framework;
    using Payloads;


    [TestFixture]
    public class Using_a_source
    {
        [Test]
        public async Task Should_provide_the_requested_context()
        {
        }


        public interface SourceContext<out TSource, out TContext> :
            SourceContext<TSource>
        {
            TContext Context { get; }
        }


        class Host :
            ISource<ConnectionContext>,
            ISupervisor
        {
            readonly ICacheContextSupervisor<ConnectionContext> _supervisor;
            readonly Context _context;

            public Host()
            {
                _context = new Context();

                _supervisor = new CacheContextSupervisor<ConnectionContext>(new ConnectionContextFactory(_context));
            }

            public Task Send(IPipe<ConnectionContext> pipe, CancellationToken cancellationToken = default(CancellationToken))
            {
                return _supervisor.Send(pipe, cancellationToken);
            }

            public void Probe(ProbeContext context)
            {
            }


            class ConnectionContextFactory :
                IPipeContextFactory<ConnectionContext>
            {
                readonly Context _context;

                public ConnectionContextFactory(Context context)
                {
                    _context = context;
                }

                PipeContextHandle<ConnectionContext> IPipeContextFactory<ConnectionContext>.CreateContext(ISupervisor supervisor)
                {
                    return supervisor.AddContext<ConnectionContext>(new Connection(_context, supervisor.Stopping));
                }

                ActivePipeContextHandle<ConnectionContext> IPipeContextFactory<ConnectionContext>.CreateActiveContext(ISupervisor supervisor,
                    PipeContextHandle<ConnectionContext> context, CancellationToken cancellationToken)
                {
                    return supervisor.AddActiveContext(context, CreateSharedConnection(context.Context, cancellationToken));
                }

                async Task<ConnectionContext> CreateSharedConnection(Task<ConnectionContext> context, CancellationToken cancellationToken)
                {
                    var connectionContext = await context.ConfigureAwait(false);

                    var sharedConnection = new SharedConnection(connectionContext, cancellationToken);

                    return sharedConnection;
                }
            }


            class Context :
                BasePipeContext,
                HostContext
            {
            }


            Task IAgent.Ready => _supervisor.Ready;

            Task IAgent.Completed => _supervisor.Completed;

            CancellationToken IAgent.Stopping => _supervisor.Stopping;

            Task IAgent.Stop(StopContext context)
            {
                return _supervisor.Stop(context);
            }

            void ISupervisor.Add(IAgent agent)
            {
                _supervisor.Add(agent);
            }
        }


        class ModelSource :
            ISource<ModelContext>
        {
            readonly ISource<ConnectionContext> _connectionContextSource;

            public ModelSource(ISource<ConnectionContext> connectionContextSource)
            {
                _connectionContextSource = connectionContextSource;
            }

            public Task Send(IPipe<ModelContext> pipe, CancellationToken cancellationToken = default(CancellationToken))
            {
                var modelPipe = new ModelPipeAdapter(pipe);

                return _connectionContextSource.Send(modelPipe, cancellationToken);
            }

            public void Probe(ProbeContext context)
            {
            }


            class ModelPipeAdapter :
                IPipe<ConnectionContext>
            {
                readonly IPipe<ModelContext> _pipe;

                public ModelPipeAdapter(IPipe<ModelContext> pipe)
                {
                    _pipe = pipe;
                }

                public Task Send(ConnectionContext context)
                {
                    var model = new Model(context);

                    return _pipe.Send(model);
                }

                public void Probe(ProbeContext context)
                {
                }
            }
        }


        public interface SourceContext<out TSource> :
            PipeContext
        {
            TSource Source { get; }
        }


        interface HostContext :
            PipeContext
        {
        }


        interface ConnectionContext :
            PipeContext
        {
            HostContext HostContext { get; }
        }


        class Connection :
            BasePipeContext,
            ConnectionContext
        {
            public Connection(HostContext hostContext, CancellationToken cancellationToken)
                : base(new PayloadCacheScope(hostContext), cancellationToken)
            {
                HostContext = hostContext;
            }

            public HostContext HostContext { get; }
        }


        class SharedConnection :
            BasePipeContext,
            ConnectionContext
        {
            readonly ConnectionContext _context;

            public SharedConnection(ConnectionContext context, CancellationToken cancellationToken)
                : base(new PayloadCacheScope(context), cancellationToken)
            {
                _context = context;
            }

            public HostContext HostContext => _context.HostContext;
        }


        interface ModelContext :
            PipeContext
        {
            ConnectionContext ConnectionContext { get; }
        }


        class Model :
            BasePipeContext,
            ModelContext
        {
            public Model(ConnectionContext connectionContext)
                : base(new PayloadCacheScope(connectionContext), connectionContext.CancellationToken)
            {
                ConnectionContext = connectionContext;
            }

            public ConnectionContext ConnectionContext { get; }
        }


        interface CreateReceiveEndpointContext :
            PipeContext
        {
        }
    }
}