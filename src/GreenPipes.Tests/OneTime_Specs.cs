// Copyright 2012-2019 Chris Patterson
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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;


    [TestFixture]
    public class OneTime_Specs
    {
        [Test]
        public async Task Should_only_invoke_the_method_once()
        {
            var callCount = 0;
            var totalCount = 0;

            IPipe<IInputContext> pipe = Pipe.New<IInputContext>(cfg =>
            {
                cfg.UseExecuteAsync(async x =>
                {
                    await x.OneTimeSetup<Secret<IInputContext>>(async key =>
                    {
                        Interlocked.Increment(ref callCount);
                    }, () => new MySecret<IInputContext>());

                    Interlocked.Increment(ref totalCount);
                });
            });

            var context = new InputContext("Hello");

            Task[] tasks = Enumerable.Range(0, 50)
                .Select(index => Task.Run(async () => await pipe.Send(context)))
                .ToArray();

            await Task.WhenAll(tasks);

            Assert.That(callCount, Is.EqualTo(1));
            Assert.That(totalCount, Is.EqualTo(50));
        }


        public interface Secret<T>
        {
        }


        public class MySecret<T> :
            Secret<T>
        {
        }
    }
}
