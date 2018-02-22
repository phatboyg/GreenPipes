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
    using System.Threading.Tasks;
    using GreenPipes.Internals.Extensions;
    using NUnit.Framework;


    [TestFixture]
    public class Bind_Specs
    {
        [Test]
        public async Task Should_include_the_bound_context_in_the_pipe()
        {
            Filter filter = null;
            IPipe<InputContext> pipe = Pipe.New<InputContext>(cfg =>
            {
                cfg.UseBind(x => x.Source(new ThingFactory(), p =>
                {
                    p.ContextPipe.UseExecuteAsync(context => Console.Out.WriteLineAsync($"ContextPipe: {context.Value}"));
                    filter = new Filter();
                    p.UseFilter(filter);
                }));
            });

            await pipe.Send(new InputContext("Input"));

            await filter.GotTheThing.UntilCompletedOrTimeout(TimeSpan.FromSeconds(5));
        }


        class Filter :
            IFilter<BindContext<InputContext, Thing>>
        {
            readonly TaskCompletionSource<Thing> _completed;

            public Filter()
            {
                _completed = new TaskCompletionSource<Thing>();
            }

            public async Task Send(BindContext<InputContext, Thing> context, IPipe<BindContext<InputContext, Thing>> next)
            {
                _completed.SetResult(context.SourceContext);

                await next.Send(context);
            }

            public void Probe(ProbeContext context)
            {
            }

            public Task<Thing> GotTheThing => _completed.Task;
        }


        class ThingFactory :
            IPipeContextSource<Thing, InputContext>
        {
            public Task Send(InputContext context, IPipe<Thing> pipe)
            {
                var thing = new Thing {Value = "Rock!"};

                return pipe.Send(thing);
            }

            public void Probe(ProbeContext context)
            {
            }
        }


        class Thing :
            BasePipeContext,
            PipeContext
        {
            public string Value { get; set; }
        }
    }
}