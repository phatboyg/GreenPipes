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
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Control;
    using NUnit.Framework;
    using Pipes;


    [TestFixture]
    public class Specifying_a_rate_limit
    {
        [Test, Explicit]
        public async Task Should_only_do_n_messages_per_interval()
        {
            int count = 0;
            IPipe<Input> pipe = Pipe.New<Input>(x =>
            {
                x.UseRateLimit(10, TimeSpan.FromSeconds(1));
                x.UseExecute(payload =>
                {
                    Interlocked.Increment(ref count);
                });
            });

            var context = new Input("Hello");

            var timer = Stopwatch.StartNew();

            Task[] tasks = Enumerable.Range(0, 101)
                .Select(index => Task.Run(async () => await pipe.Send(context)))
                .ToArray();

            await Task.WhenAll(tasks);

            timer.Stop();

            Assert.That(timer.ElapsedMilliseconds, Is.GreaterThan(9500));
        }

        [Test, Explicit]
        public async Task Should_allow_dynamic_reconfiguration_up()
        {
            var router = new CommandRouter();
            int count = 0;
            IPipe<Input> pipe = Pipe.New<Input>(x =>
            {
                x.UseRateLimit(10, TimeSpan.FromSeconds(1), router);
                x.UseExecute(payload =>
                {
                    Interlocked.Increment(ref count);
                });
            });

            await router.SetRateLimit(100);

            var context = new Input("Hello");

            var timer = Stopwatch.StartNew();

            Task[] tasks = Enumerable.Range(0, 101)
                .Select(index => Task.Run(async () => await pipe.Send(context)))
                .ToArray();

            await Task.WhenAll(tasks);

            timer.Stop();

            Assert.That(timer.ElapsedMilliseconds, Is.LessThan(2000));
        }

        [Test, Explicit]
        public async Task Should_allow_dynamic_reconfiguration_down()
        {
            var router = new CommandRouter();
            int count = 0;
            IPipe<Input> pipe = Pipe.New<Input>(x =>
            {
                x.UseRateLimit(100, TimeSpan.FromSeconds(1), router);
                x.UseExecute(payload =>
                {
                    Interlocked.Increment(ref count);
                });
            });

            await router.SetRateLimit(10);

            var context = new Input("Hello");

            var timer = Stopwatch.StartNew();

            Task[] tasks = Enumerable.Range(0, 101)
                .Select(index => Task.Run(async () => await pipe.Send(context)))
                .ToArray();

            await Task.WhenAll(tasks);

            timer.Stop();

            Assert.That(timer.ElapsedMilliseconds, Is.GreaterThan(9500));
        }

        [Test, Explicit]
        public async Task Should_count_success_and_failure_as_same()
        {
            int count = 0;
            IPipe<Input> pipe = Pipe.New<Input>(x =>
            {
                x.UseRateLimit(10, TimeSpan.FromSeconds(1));
                x.UseExecute(payload =>
                {
                    var index = Interlocked.Increment(ref count);
                    if (index % 2 == 0)
                        throw new IntentionalTestException();
                });
            });

            var context = new Input("Hello");

            var timer = Stopwatch.StartNew();

            Task[] tasks = Enumerable.Range(0, 101)
                .Select(index => Task.Run(async () =>
                {
                    try
                    {
                        await pipe.Send(context);
                    }
                    catch (Exception)
                    {
                    }
                }))
                .ToArray();

            await Task.WhenAll(tasks);

            timer.Stop();

            Assert.That(timer.ElapsedMilliseconds, Is.GreaterThan(9500));
        }
    }
}