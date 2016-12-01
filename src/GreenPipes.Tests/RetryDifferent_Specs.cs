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
    using NUnit.Framework;


    [TestFixture]
    public class RetryDifferent_Specs
    {
        [Test]
        public void Should_retry_the_specified_times_and_fail()
        {
            var count = 0;
            IPipe<TestContext> pipe = Pipe.New<TestContext>(x =>
            {
                x.UseRetry(r =>
                {
                    r.Handle<IntentionalTestException>();
                    r.Immediate(10);
                });

                x.UseRetry(r =>
                {
                    r.Handle<InvalidOperationException>();
                    r.Immediate(5);
                });

                x.UseExecute(payload =>
                {
                    var current = Interlocked.Increment(ref count);
                    if (current % 2 == 0)
                        throw new IntentionalTestException("Kaboom!");

                    throw new InvalidOperationException("Expected, but unwarranted");
                });
            });

            var context = new TestContext();

            Assert.That(async () => await pipe.Send(context), Throws.TypeOf<IntentionalTestException>());

            Assert.That(count, Is.EqualTo(22));
        }


        class TestContext :
            BasePipeContext,
            PipeContext
        {
        }
    }
}