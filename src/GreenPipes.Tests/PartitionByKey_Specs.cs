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
namespace MassTransit.Tests.Pipeline
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GreenPipes;
    using GreenPipes.Tests;
    using NUnit.Framework;


    [TestFixture]
    public class Partitioning_a_consumer_by_key
    {
        [Test]
        public async Task Should_use_a_partitioner_for_consistency()
        {
            var completed = new TaskCompletionSource<int>();
            var count = 0;
            IPipe<InputContext> pipe = Pipe.New<InputContext>(x =>
            {
                x.UsePartitioner(8, context => (string)context.Value);
                x.UseExecute(payload =>
                {
                    if (Interlocked.Increment(ref count) == Limit)
                        completed.TrySetResult(Limit);
                });
            });

            await Task.WhenAll(Enumerable.Range(0, Limit).Select(index => pipe.Send(new InputContext(index.ToString()))));

            await completed.Task;

            Assert.AreEqual(Limit, count);

            Console.WriteLine("Processed: {0}", count);
        }

        const int Limit = 100;
    }
}