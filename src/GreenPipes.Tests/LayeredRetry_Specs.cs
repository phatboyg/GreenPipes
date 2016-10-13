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
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using NUnit.Framework;
    using Pipes;


    [TestFixture]
    public class Layering_retry_components_into_a_set   
    {
        [Test, Explicit]
        public async Task Should_support_interaction_between_filters()
        {

            var myFilter = new MyFilter();

            IPipeRouter router = new PipeRouter();

            var pipe = Pipe.New<InputContext>(cfg =>
            {
                cfg.UseConcurrencyLimit(10, router);
                cfg.UseCircuitBreaker(cb =>
                {
                    cb.ActiveThreshold = 5;
                    cb.TrackingPeriod = TimeSpan.FromSeconds(60);
                    cb.TripThreshold = 25;
                    cb.ResetInterval = TimeSpan.FromSeconds(30);

                    cb.Router = router;
                });
                cfg.UseRetry(x => x.Immediate(1));

                cfg.UseFilter(myFilter);
            });

            myFilter.Throw = true;

            router.ConnectPipe(Pipe.New<EventContext<CircuitBreakerOpened>>(x => x.UseFilter(new MyController(router))));


            await Task.WhenAll(Enumerable.Range(0, 140).Select(async index =>
            {
                try
                {
                    await pipe.Send(new InputContext("Hello"));
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync($"{DateTime.Now:mm:ss:fff} - Faulted: {ex.Message}");
                }
            }));
        }


        class MyController :
            IFilter<EventContext<CircuitBreakerOpened>>
        {
            readonly IPipeRouter _router;

            public MyController(IPipeRouter router)
            {
                _router = router;
            }

            public async Task Send(EventContext<CircuitBreakerOpened> context, IPipe<EventContext<CircuitBreakerOpened>> next)
            {
                await Console.Out.WriteLineAsync($"Changing concurrency limit in response to circuit breaker");

                await _router.SetConcurrencyLimit(1);

                await next.Send(context);
            }

            public void Probe(ProbeContext context)
            {
                
            }
        }


        class MyFilter :
            IFilter<InputContext>
        {
            public bool Throw { get; set; }

            public async Task Send(InputContext context, IPipe<InputContext> next)
            {
                await Console.Out.WriteLineAsync($"{context.Value} : {DateTime.Now:hh:mm:ss:fff}");

                await Task.Delay(1000);

                if (Throw)
                    throw new IntentionalTestException("MyFilter is throwing");

            }

            public void Probe(ProbeContext context)
            {
            }
        }


        
    }
}