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
    namespace Rescues
    {
        using System;
        using System.Threading;
        using System.Threading.Tasks;
        using NUnit.Framework;
        using Payloads;


        [TestFixture]
        public class RescueContext_Specs
        {
            [Test]
            public async Task Should_invoke_the_rescue_pipe()
            {
                var count = 0;
                IPipe<SomeContext> pipe = Pipe.New<SomeContext>(x =>
                {
                    x.UseRescue((cxt, ex) => (SomeRescueContext)new SomeRescueContextImpl(cxt, ex), r =>
                    {
                        r.UseExecute(c => Interlocked.Increment(ref count));

                        r.Handle<IntentionalTestException>();
                    });

                    x.UseExecute(cxt =>
                    {
                        throw new IntentionalTestException("Kaboom!");
                    });
                });

                var context = new SomeContextImpl();

                await pipe.Send(context);

                Assert.That(count, Is.EqualTo(1));
            }
        }


        interface SomeContext :
            PipeContext
        {
        }


        class SomeContextImpl :
            BasePipeContext,
            SomeContext
        {
        }


        interface SomeRescueContext :
            SomeContext
        {
            Exception Exception { get; }
        }


        class SomeRescueContextImpl :
            BasePipeContext,
            SomeRescueContext
        {
            public SomeRescueContextImpl(SomeContext context, Exception exception)
                : base(context)
            {
                Exception = exception;
            }

            public Exception Exception { get; }
        }
    }
}