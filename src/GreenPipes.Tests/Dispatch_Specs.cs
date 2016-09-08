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
    using System.Linq;
    using System.Threading.Tasks;
    using Filters;
    using NUnit.Framework;
    using Payloads;


    [TestFixture]
    public class Dispatch_Specs
    {
        [Test]
        public async Task Dispatching_a_pipe_by_type()
        {
            IPipe<Input> pipe = Pipe.New<Input>(x =>
            {
                // this needs to be moved from Factory() to some type of management glue that
                // includes the factory, and hooks to connect/add/review
                x.UseDispatch(new InputConverterFactory(), d =>
                {
                    d.Pipe<Input<string>>(p =>
                    {
                        p.UseExecute(context => Console.WriteLine(context.Value));
                    });
                });
            });

            await pipe.Send(new Input("Hello"));
        }
    }


    public class Input :
        BasePipeContext,
        PipeContext
    {
        public Input(object value)
            : base(new PayloadCache())
        {
            Value = value;
        }

        public object Value { get; }

        public bool TryGetContext<T>(out T result)
            where T : class
        {
            if (typeof(T).IsAssignableFrom(typeof(Input<string>)))
            {
                result = new Input<string>(Value.ToString()) as T;

                return result != null;
            }

            result = null;
            return false;
        }
    }


    public class Input<T> :
        BasePipeContext,
        PipeContext
        where T : class
    {
        public Input(T value)
            : base(new PayloadCache())
        {
            Value = value;
        }

        public T Value { get; }
    }


    class InputConverterFactory :
        IPipeContextConverterFactory<Input>
    {
        IPipeContextConverter<Input, TOutput> IPipeContextConverterFactory<Input>.GetConverter<TOutput>()
        {
            var innerType = typeof(TOutput).GetClosingArguments(typeof(Input<>)).Single();

            return (IPipeContextConverter<Input, TOutput>)Activator.CreateInstance(typeof(Converter<>).MakeGenericType(innerType));
        }


        class Converter<T> :
            IPipeContextConverter<Input, Input<T>>
            where T : class
        {
            bool IPipeContextConverter<Input, Input<T>>.TryConvert(Input input, out Input<T> output)
            {
                return input.TryGetContext(out output);
            }
        }
    }
}