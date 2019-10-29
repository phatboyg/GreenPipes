// Copyright 2012-2016 Chris Patterson
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
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using NUnit.Framework;
    using Pipes;
    using Util;


    [TestFixture]
    public class Using_a_circuit_breaker
    {
        [Test]
        public async Task Should_allow_the_first_call()
        {
            var router = new PipeRouter();

            var count = 0;
            IPipe<TestContext> pipe = Pipe.New<TestContext>(x =>
            {
                x.UseCircuitBreaker(v =>
                {
                    v.ResetInterval = TimeSpan.FromSeconds(60);
                    v.Router = router;
                });
                x.UseExecute(payload =>
                {
                    Interlocked.Increment(ref count);

                    throw new IntentionalTestException();
                });
            });

            TaskCompletionSource<CircuitBreakerOpened> opened = TaskUtil.GetTask<CircuitBreakerOpened>();
            var observeCircuitBreaker = Pipe.Execute<EventContext<CircuitBreakerOpened>>(x => opened.TrySetResult(x.Event));
            router.ConnectPipe(observeCircuitBreaker);

            var context = new TestContext();

            for (var i = 0; i < 100; i++)
                Assert.That(async () => await pipe.Send(context).ConfigureAwait(false), Throws.TypeOf<IntentionalTestException>());

            Assert.That(count, Is.EqualTo(6));

            await opened.Task;
        }


        class TestContext :
            BasePipeContext,
            PipeContext
        {
        }
    }
}
