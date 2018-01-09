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
            BasePipeContext,
            HostContext,
            ISource<ConnectionContext>
        {
            public Task Send(IPipe<ConnectionContext> pipe, CancellationToken cancellationToken = default(CancellationToken))
            {
                var connection = new Connection(this, cancellationToken);

                return pipe.Send(connection);
            }

            public void Probe(ProbeContext context)
            {
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