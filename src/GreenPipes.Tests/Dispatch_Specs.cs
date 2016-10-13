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
    using Filters;
    using NUnit.Framework;


    [TestFixture]
    public class Dispatch_Specs
    {
        [Test]
        public async Task Dispatching_a_pipe_by_type()
        {
            IPipe<InputContext> pipe = Pipe.New<InputContext>(cfg =>
            {
                // this needs to be moved from Factory() to some type of management glue that
                // includes the factory, and hooks to connect/add/review
                cfg.UseDispatch(new InputConverterFactory(), d =>
                {
                    d.Pipe<InputContext<string>>(p =>
                    {
                        p.UseExecute(cxt => Console.WriteLine(cxt.Value));
                    });
                });
            });

            await pipe.Send(new InputContext("Hello"));
        }
    }


    public class InputContext :
        BasePipeContext,
        PipeContext
    {
        public InputContext(object value)
        {
            Value = value;
        }

        public object Value { get; }

        public bool TryGetContext<T>(out T result)
            where T : class
        {
            if (typeof(T).IsAssignableFrom(typeof(InputContext<string>)))
            {
                result = new InputContext<string>(Value.ToString()) as T;

                return result != null;
            }

            result = null;
            return false;
        }
    }


    public class InputContext<T> :
        BasePipeContext,
        PipeContext
        where T : class
    {
        public InputContext(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }


    class InputConverterFactory :
        IPipeContextConverterFactory<InputContext>
    {
        IPipeContextConverter<InputContext, TOutput> IPipeContextConverterFactory<InputContext>.GetConverter<TOutput>()
        {
            var innerType = typeof(TOutput).GetClosingArguments(typeof(InputContext<>)).Single();

            return (IPipeContextConverter<InputContext, TOutput>)Activator.CreateInstance(typeof(Converter<>).MakeGenericType(innerType));
        }


        class Converter<T> :
            IPipeContextConverter<InputContext, InputContext<T>>
            where T : class
        {
            bool IPipeContextConverter<InputContext, InputContext<T>>.TryConvert(InputContext inputContext, out InputContext<T> output)
            {
                return inputContext.TryGetContext(out output);
            }
        }
    }
}