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
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class Using_the_log_filter
    {
        [Test]
        public async Task Should_call_log_actions()
        {
            var executionCount = 0;
            var startLogCount = 0;
            var completeLogCount = 0;
            var errorLogCount = 0;
            IPipe<TestContext> pipe = Pipe.New<TestContext>(x =>
            {
                x.UseLogExt(async r => { startLogCount++; }, async r => { completeLogCount++; }, async (r, ex) => { errorLogCount++; });
                x.UseExecute(payload =>
                {
                    executionCount++;
                    if (executionCount == 2)
                        throw new IntentionalTestException("Kaboom!");
                });
            });

            var context = new TestContext();

            await pipe.Send(context);
            Assert.That(executionCount, Is.EqualTo(1));
            Assert.That(startLogCount, Is.EqualTo(1));
            Assert.That(completeLogCount, Is.EqualTo(1));
            Assert.That(errorLogCount, Is.EqualTo(0));

            Assert.That(async () => await pipe.Send(context), Throws.TypeOf<IntentionalTestException>());

            Assert.That(executionCount, Is.EqualTo(2));
            Assert.That(startLogCount, Is.EqualTo(2));
            Assert.That(completeLogCount, Is.EqualTo(1));
            Assert.That(errorLogCount, Is.EqualTo(1));
        }

        class TestContext :
            BasePipeContext,
            PipeContext
        {
        }
    }
}