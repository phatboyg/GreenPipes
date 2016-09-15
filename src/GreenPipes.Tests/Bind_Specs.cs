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
    using NUnit.Framework;


    [TestFixture]
    public class Bind_Specs
    {
        [Test]
        public async Task Binding_a_pipe_to_a_target()
        {
            IPipe<InputContext> pipe = Pipe.New<InputContext>(cfg =>
            {
                cfg.UseBind(x => x.Target<Thing>(p =>
                {
                    p.SetTargetFactory(new ThingFactory());

                    p.ContextPipe.UseExecuteAsync(context => Console.Out.WriteLineAsync($"ContextPipe: {context.Value}"));
                    p.UseFilter(new Filter());
                }));
            });

            await pipe.Send(new InputContext("Input"));
        }


        class Filter :
            IFilter<BindContext<InputContext, Thing>>
        {
            public async Task Send(BindContext<InputContext, Thing> context, IPipe<BindContext<InputContext, Thing>> next)
            {
                await Console.Out.WriteLineAsync($"Hello, World: {context.Context.Value} - {context.Target.Value}");

                await next.Send(context);
            }

            public void Probe(ProbeContext context)
            {
            }
        }


        class ThingFactory :
            ITargetFactory<Thing>
        {
            public Task<Thing> CreateSource<T>(T context) where T : class, PipeContext
            {
                return Task.FromResult(new Thing {Value = "Rock!"});
            }

            public void Probe(ProbeContext context)
            {
            }
        }


        class Thing
        {
            public string Value { get; set; }
        }
    }
}