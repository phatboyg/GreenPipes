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
            IPipe<IInputContext> pipe = Pipe.New<IInputContext>(cfg =>
            {
                // this needs to be moved from Factory() to some type of management glue that
                // includes the factory, and hooks to connect/add/review
                cfg.UseDispatch(new InputConverterFactory(), d =>
                {
                    d.Pipe<IInputContext<string>>(p =>
                    {
                        p.UseExecute(cxt => Console.WriteLine(cxt.Value));
                    });
                });
            });

            await pipe.Send(new InputContext("Hello"));
        }
    }


    public interface IInputContext :
        PipeContext
    {
        bool TryGetContext<T>(out T result)
            where T : class;
    }


    public class InputContext :
        BasePipeContext,
        IInputContext
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
                result = new InputContext<string>(this, Value.ToString()) as T;

                return result != null;
            }

            result = null;
            return false;
        }
    }


    public interface IInputContext<out T> :
        IInputContext
        where T : class
    {
        T Value { get; }
    }


    public class InputContext<T> :
        ProxyPipeContext,
        IInputContext<T>
        where T : class
    {
        readonly IInputContext _inputContext;

        public InputContext(IInputContext inputContext, T value)
            : base(inputContext)
        {
            _inputContext = inputContext;
            Value = value;
        }

        public T Value { get; }

        public bool TryGetContext<T1>(out T1 result)
            where T1 : class
        {
            return _inputContext.TryGetContext(out result);
        }
    }


    class InputConverterFactory :
        IPipeContextConverterFactory<IInputContext>
    {
        IPipeContextConverter<IInputContext, TOutput> IPipeContextConverterFactory<IInputContext>.GetConverter<TOutput>()
        {
            var innerType = typeof(TOutput).GetClosingArguments(typeof(IInputContext<>)).Single();

            return (IPipeContextConverter<IInputContext, TOutput>)Activator.CreateInstance(typeof(Converter<>).MakeGenericType(innerType));
        }


        class Converter<T> :
            IPipeContextConverter<IInputContext, IInputContext<T>>
            where T : class
        {
            bool IPipeContextConverter<IInputContext, IInputContext<T>>.TryConvert(IInputContext inputContext, out IInputContext<T> output)
            {
                return inputContext.TryGetContext(out output);
            }
        }
    }
}