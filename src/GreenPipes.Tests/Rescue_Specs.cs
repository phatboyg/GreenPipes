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
    using NUnit.Framework;


    [TestFixture]
    public class Using_the_rescue_filter
    {
        class TestContext :
            BasePipeContext,
            PipeContext
        {
            public TestContext()
            {
            }

            public TestContext(TestContext testContext)
                : base(testContext)
            {
            }
        }


        class TestExceptionContext :
            TestContext
        {
            public TestExceptionContext(TestContext context, Exception exception)
                : base(context)
            {
                Exception = exception;
            }

            public Exception Exception { get; }
        }


        [Test]
        public async Task Should_invoke_the_rescue_pipe()
        {
            var count = 0;
            IPipe<TestContext> pipe = Pipe.New<TestContext>(x =>
            {
                x.UseRescue(Pipe.New<TestExceptionContext>(r =>
                {
                    r.UseExecute(c => Interlocked.Increment(ref count));
                }), (cxt, ex) => new TestExceptionContext(cxt, ex), r => r.Handle<IntentionalTestException>());

                x.UseExecute(cxt =>
                {
                    throw new IntentionalTestException("Kaboom!");
                });
            });

            var context = new TestContext();

            await pipe.Send(context);

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_skip_if_filtered_exception()
        {
            var count = 0;
            IPipe<TestContext> pipe = Pipe.New<TestContext>(x =>
            {
                x.UseRescue(Pipe.New<TestExceptionContext>(r =>
                {
                    r.UseExecute(c => Interlocked.Increment(ref count));
                }), (cxt, ex) => new TestExceptionContext(cxt, ex), r => r.Ignore<IntentionalTestException>());

                x.UseExecute(cxt =>
                {
                    throw new IntentionalTestException("Kaboom!");
                });
            });

            var context = new TestContext();

            Assert.That(async () => await pipe.Send(context), Throws.TypeOf<IntentionalTestException>());
        }
    }
}