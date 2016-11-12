﻿// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using Contracts;
    using NUnit.Framework;
    using Pipes;


    [TestFixture]
    public class ConcurrencyLimit_Specs
    {
        [Test]
        public async Task Should_allow_just_enough_threads_at_once()
        {
            var currentCount = 0;
            var maxCount = 0;

            IPipe<InputContext> pipe = Pipe.New<InputContext>(cfg =>
            {
                cfg.UseConcurrencyLimit(32);
                cfg.UseExecuteAsync(async cxt =>
                {
                    var current = Interlocked.Increment(ref currentCount);
                    while (current > maxCount)
                        Interlocked.CompareExchange(ref maxCount, current, maxCount);

                    await Task.Delay(10);

                    Interlocked.Decrement(ref currentCount);
                });
            });

            var context = new InputContext("Hello");

            Task[] tasks = Enumerable.Range(0, 500)
                .Select(index => Task.Run(async () => await pipe.Send(context)))
                .ToArray();

            await Task.WhenAll(tasks);

            Assert.That(maxCount, Is.EqualTo(32));
        }

        [Test]
        public async Task Should_allow_reconfiguration_at_runtime()
        {
            var currentCount = 0;
            var maxCount = 0;

            IPipeRouter dynamicRouter = new PipeRouter();

            IPipe<InputContext> pipe = Pipe.New<InputContext>(cfg =>
            {
                cfg.UseConcurrencyLimit(1, dynamicRouter);
                cfg.UseExecuteAsync(async cxt =>
                {
                    var current = Interlocked.Increment(ref currentCount);
                    while (current > maxCount)
                        Interlocked.CompareExchange(ref maxCount, current, maxCount);

                    await Task.Delay(10);

                    Interlocked.Decrement(ref currentCount);
                });
            });

            await dynamicRouter.SendCommand<SetConcurrencyLimit>(new
            {
                ConcurrencyLimit = 32
            });

            var context = new InputContext("Hello");

            Task[] tasks = Enumerable.Range(0, 500)
                .Select(index => Task.Run(async () => await pipe.Send(context)))
                .ToArray();

            await Task.WhenAll(tasks);

            Assert.That(maxCount, Is.EqualTo(32));
        }

        [Test]
        public async Task Should_prevent_too_many_threads_at_one_time()
        {
            var currentCount = 0;
            var maxCount = 0;

            IPipe<InputContext> pipe = Pipe.New<InputContext>(cfg =>
            {
                cfg.UseConcurrencyLimit(1);
                cfg.UseExecuteAsync(async cxt =>
                {
                    var current = Interlocked.Increment(ref currentCount);
                    while (current > maxCount)
                        Interlocked.CompareExchange(ref maxCount, current, maxCount);

                    await Task.Delay(10);

                    Interlocked.Decrement(ref currentCount);
                });
            });

            var context = new InputContext("Hello");

            Task[] tasks = Enumerable.Range(0, 50)
                .Select(index => Task.Run(async () => await pipe.Send(context)))
                .ToArray();

            await Task.WhenAll(tasks);

            Assert.That(maxCount, Is.EqualTo(1));
        }
    }
}