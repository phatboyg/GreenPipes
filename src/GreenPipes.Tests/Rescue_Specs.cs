// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using Payloads;


    [TestFixture]
    public class Using_the_rescue_filter
    {
        class A :
            BasePipeContext,
            PipeContext
        {
            public A()
                : base(new PayloadCache())
            {
            }

            public A(A a)
                : base(a)
            {
            }
        }


        class AX :
            A
        {
            public AX(A context, Exception exception)
                : base(context)
            {
                Exception = exception;
            }

            public Exception Exception { get; }
        }


        [Test]
        public async Task Should_invoke_the_rescue_pipe()
        {
            int count = 0;
            IPipe<A> pipe = Pipe.New<A>(x =>
            {
                x.UseRescue(Pipe.New<AX>(r =>
                {
                    r.UseExecute(c => Interlocked.Increment(ref count));
                }), (a, ex) => new AX(a, ex), r => r.Handle<IntentionalTestException>());

                x.UseExecute(payload =>
                {
                    throw new IntentionalTestException("Kaboom!");
                });
            });

            var context = new A();

            await pipe.Send(context);

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_skip_if_filtered_exception()
        {
            int count = 0;
            IPipe<A> pipe = Pipe.New<A>(x =>
            {
                x.UseRescue(Pipe.New<AX>(r =>
                {
                    r.UseExecute(c => Interlocked.Increment(ref count));
                }), (a, ex) => new AX(a, ex), r => r.Ignore<IntentionalTestException>());

                x.UseExecute(payload =>
                {
                    throw new IntentionalTestException("Kaboom!");
                });
            });

            var context = new A();

            Assert.That(async () => await pipe.Send(context), Throws.TypeOf<IntentionalTestException>());
        }
    }
}