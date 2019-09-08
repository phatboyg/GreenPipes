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
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Contexts;
    using Microsoft.Extensions.Caching.Memory;
    using NUnit.Framework;


    [TestFixture]
    public class Using_a_complex_pipe_with_cache_support
    {
        [Test]
        public async Task Should_support_awesome_caching_speed()
        {
            IPipe<RequestContext> pipe = CreateHandlerPipe();

            IRequestPipe<CheckInventory, CheckInventoryResult> requestPipe = pipe.CreateRequestPipe<CheckInventory, CheckInventoryResult>(x =>
            {
                x.UseFilter(new UpdateCacheFilter(_inventoryCache));
            });

            var checks = new CheckInventory[]
            {
                new CheckInventory() {ItemNumber = "ABC123", Quantity = 2},
                new CheckInventory() {ItemNumber = "DEF456", Quantity = 4},
                new CheckInventory() {ItemNumber = "GHI789", Quantity = 6},
            };

            var timer = Stopwatch.StartNew();
            for (var i = 0; i < 100; i++)
                await requestPipe.Send(checks[i % checks.Length]).Result();

            timer.Stop();

            Console.WriteLine("Total time: {0}ms", timer.ElapsedMilliseconds);
            Console.WriteLine("Total calls: {0}", _totalCalls);

            Assert.That(timer.Elapsed, Is.LessThan(TimeSpan.FromMilliseconds(1000)));
        }

        [Test]
        public async Task Should_support_pipe_configuration_on_the_result()
        {
            IPipe<RequestContext> pipe = CreateHandlerPipe();

            IRequestPipe<CheckInventory, CheckInventoryResult> requestPipe = pipe.CreateRequestPipe<CheckInventory, CheckInventoryResult>(x =>
            {
                x.UseFilter(new UpdateCacheFilter(_inventoryCache));
            });

            var checkInventory = new CheckInventory() {ItemNumber = "ABC123", Quantity = 2};

            var timer = Stopwatch.StartNew();
            var result = await requestPipe.Send(checkInventory).Result();
            timer.Stop();

            Assert.That(result.ItemNumber, Is.EqualTo(checkInventory.ItemNumber));
            Assert.That(result.QuantityOnHand, Is.EqualTo(checkInventory.ItemNumber.GetHashCode() % 100));
            Assert.That(timer.Elapsed, Is.GreaterThan(TimeSpan.FromMilliseconds(200)));

            timer = Stopwatch.StartNew();
            result = await requestPipe.Send(checkInventory).Result();
            timer.Stop();

            Assert.That(result.ItemNumber, Is.EqualTo(checkInventory.ItemNumber));
            Assert.That(result.QuantityOnHand, Is.EqualTo(checkInventory.ItemNumber.GetHashCode() % 100));
            Assert.That(timer.Elapsed, Is.LessThan(TimeSpan.FromMilliseconds(20)));
        }

        static InventoryCache _inventoryCache;
        static int _totalCalls;

        static IPipe<RequestContext> CreateHandlerPipe()
        {
            _inventoryCache = new InventoryCache();

            _totalCalls = 0;

            IPipe<RequestContext> pipe = Pipe.New<RequestContext>(cfg =>
            {
                cfg.UseDispatch(new RequestConverterFactory(), d =>
                {
                    // The cache is on a separate pipe, to allow concurrent execution to adjacent pipe below
                    d.Handle<CheckInventory>(h =>
                    {
                        h.UseFilter(new ResultCacheFilter(_inventoryCache));
                    });

                    // The check filter is going to call the API to get the actual inventory amount.
                    d.Handle<CheckInventory>(h =>
                    {
                        h.UseRateLimit(10);
                        h.UseConcurrencyLimit(2);
                        h.UseFilter(new CheckInventoryFilter());
                    });
                });
            });
            return pipe;
        }


        class CheckInventory
        {
            public string ItemNumber { get; set; }
            public int Quantity { get; set; }
        }


        [Serializable]
        public class CheckInventoryResult
        {
            public DateTime Timestamp { get; set; }
            public string ItemNumber { get; set; }
            public int QuantityOnHand { get; set; }
        }


        class InventoryCache
        {
            readonly MemoryCache _cache;

            public InventoryCache()
            {
                _cache = new MemoryCache(new MemoryCacheOptions());
            }

            public void Set(CheckInventoryResult result)
            {
                _cache.Set(result.ItemNumber, result, result.Timestamp + TimeSpan.FromMinutes(5));
            }

            public bool TryGet(string itemNumber, out CheckInventoryResult result)
            {
                var obj = _cache.Get(itemNumber);
                if (obj != null)
                    result = obj as CheckInventoryResult;
                else
                    result = default(CheckInventoryResult);

                return result != null;
            }
        }


        class CheckInventoryFilter :
            IFilter<RequestContext<CheckInventory>>
        {
            public async Task Send(RequestContext<CheckInventory> context, IPipe<RequestContext<CheckInventory>> next)
            {
                if (!context.IsCompleted)
                {
                    await Task.Delay(250).ConfigureAwait(false);

                    context.TrySetResult(new CheckInventoryResult
                    {
                        ItemNumber = context.Request.ItemNumber,
                        QuantityOnHand = context.Request.ItemNumber.GetHashCode() % 100,
                        Timestamp = DateTime.Now,
                    });
                }

                await next.Send(context).ConfigureAwait(false);
            }

            public void Probe(ProbeContext context)
            {
                var scope = context.CreateFilterScope("checkInventory");
            }
        }


        class UpdateCacheFilter :
            IFilter<ResultContext<CheckInventory, CheckInventoryResult>>
        {
            readonly InventoryCache _cache;

            public UpdateCacheFilter(InventoryCache cache)
            {
                _cache = cache;
            }

            public Task Send(ResultContext<CheckInventory, CheckInventoryResult> context, IPipe<ResultContext<CheckInventory, CheckInventoryResult>> next)
            {
                _cache.Set(context.Result);

                Console.WriteLine("Cached Result: {0}", context.Request.ItemNumber);

                return next.Send(context);
            }

            public void Probe(ProbeContext context)
            {
                context.CreateFilterScope("updateCache");
            }
        }


        class ResultCacheFilter :
            IFilter<RequestContext<CheckInventory>>
        {
            readonly InventoryCache _cache;

            public ResultCacheFilter(InventoryCache cache)
            {
                _cache = cache;
            }

            public Task Send(RequestContext<CheckInventory> context, IPipe<RequestContext<CheckInventory>> next)
            {
                if (!string.IsNullOrWhiteSpace(context.Request.ItemNumber))
                {
                    CheckInventoryResult result;
                    if (_cache.TryGet(context.Request.ItemNumber, out result))
                        context.TrySetResult(result);
                }

                return next.Send(context);
            }

            public void Probe(ProbeContext context)
            {
                var scope = context.CreateFilterScope("cache");
            }
        }
    }
}