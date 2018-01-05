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
namespace GreenPipes.Tests.Agents
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Pipes;
    using Util;


    [TestFixture]
    public class Creating_a_base_agent
    {
        [Test]
        public async Task Should_simply_stop()
        {
            IAgency agency = new Agency();

            agency.SetReady();
            
            await agency.Ready;
            
            await agency.Stop();

            await agency.Completed;
        }
        
        [Test]
        public async Task Should_stop_and_complete_with_an_agent()
        {
            IAgency agency = new Agency();

            var agentProvocateur = new AgentProvocateur();

            agency.SetReady();

            agency.Add(agentProvocateur);

            await agency.Ready;
            
            await agency.Stop();

            await agency.Completed;
        }
    }


    [TestFixture]
    public class Caching_a_pipe_context
    {
        [Test]
        public async Task Should_allow_active_instances()
        {
            ICacheContextAgency<SimpleContext> agency = new CacheContextAgency<SimpleContext>(new SimpleContextFactory());

            int count = 0;
            string lastValue = string.Empty;

            var pipe = Pipe.New<SimpleContext>(x => x.UseExecute(context =>
            {
                if (Interlocked.Increment(ref count) % 2 == 0)
                    throw new IntentionalTestException("It's odd that we throw when it's even.");

                lastValue = context.Value;
            }));

            await agency.Send(pipe);
            Assert.That(async () => await agency.Send(pipe), Throws.TypeOf<IntentionalTestException>());
            await agency.Send(pipe);

            Assert.That(lastValue, Is.EqualTo("2"));
            Assert.That(count, Is.EqualTo(3));

            await agency.Stop();

           // await agency.Completed;
        }

        [Test]
        public async Task Should_support_disconnection()
        {
            ICacheContextAgency<SimpleContext> agency = new CacheContextAgency<SimpleContext>(new SimpleContextFactory());

            agency.SetReady();

            int count = 0;
            string lastValue = string.Empty;

            var pipe = Pipe.New<SimpleContext>(x => x.UseExecute(context =>
            {
                if (Interlocked.Increment(ref count) % 2 == 0)
                    context.Invalidate();

                lastValue = context.Value;
            }));

            await agency.Send(pipe);
            await agency.Send(pipe);
            await agency.Send(pipe);

            Assert.That(lastValue, Is.EqualTo("2"));
            Assert.That(count, Is.EqualTo(3));

            await agency.Stop();

            await agency.Completed;
        }


        interface SimpleContext :
            PipeContext
        {
            string Value { get; }

            void Invalidate();
        }


        class SimpleContextImpl :
            BasePipeContext,
            SimpleContext,
            IAsyncDisposable
        {
            public string Value { get; set; }

            public void Invalidate()
            {
                OnInvalid?.Invoke(this, EventArgs.Empty);
            }

            public event EventHandler OnInvalid;

            Task IAsyncDisposable.DisposeAsync(CancellationToken cancellationToken)
            {
                return Console.Out.WriteLineAsync($"Disposing {Value}");
            }
        }


        class ActiveSimpleContext :
            SimpleContext
        {
            readonly SimpleContext _context;

            public ActiveSimpleContext(SimpleContext context, CancellationToken cancellationToken)
            {
                CancellationToken = cancellationToken;
                _context = context;
            }

            public CancellationToken CancellationToken { get; }

            bool PipeContext.HasPayloadType(Type payloadType)
            {
                return _context.HasPayloadType(payloadType);
            }

            bool PipeContext.TryGetPayload<TPayload>(out TPayload payload)
            {
                return _context.TryGetPayload(out payload);
            }

            TPayload PipeContext.GetOrAddPayload<TPayload>(PayloadFactory<TPayload> payloadFactory)
            {
                return _context.GetOrAddPayload(payloadFactory);
            }

            string SimpleContext.Value => _context.Value;

            void SimpleContext.Invalidate()
            {
                _context.Invalidate();
            }
        }


        class SimpleContextFactory :
            IAgentContextFactory<SimpleContext>
        {
            long _id;

            public AgentContextHandle<SimpleContext> CreateContext(IAgency agency)
            {
                var simpleContext = new SimpleContextImpl()
                {
                    Value = Interlocked.Increment(ref _id).ToString()
                };

                var agentContext = new AgentContext<SimpleContext>(simpleContext);

                agency.Add(agentContext);

                AgentContextHandle<SimpleContext> simpleContextHandle = agentContext;

                void SimpleContextOnInvalid(object sender, EventArgs args) => simpleContextHandle.Disavow();

                simpleContext.OnInvalid += SimpleContextOnInvalid;

                return agentContext;
            }

            public async Task<ActiveAgentContextHandle<SimpleContext>> CreateActiveContext(AgentContextHandle<SimpleContext> context,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                var existingContext = await context.Context.ConfigureAwait(false);

                var activeSimpleContext = new ActiveSimpleContext(existingContext, cancellationToken);

                ActiveAgentContextHandle<SimpleContext> activeContext = new ActiveAgentContext<SimpleContext>(context, activeSimpleContext);

                return activeContext;
            }
        }
    }
}