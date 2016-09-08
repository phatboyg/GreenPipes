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
    using System.Collections.Concurrent;
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
                x.UseDispatch(new Factory(), d =>
                {
                    d.Pipe<Input<string>>(typeof(Input<string>), p =>
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


    public class Factory :
        IPipeContextProviderFactory<Input, Type>
    {
        readonly ConcurrentDictionary<Type, IPipeContextProviderFactory<Input, Type>> _instance =
            new ConcurrentDictionary<Type, IPipeContextProviderFactory<Input, Type>>();

        IPipeContextProvider<Input, TResult> IPipeContextProviderFactory<Input, Type>.GetPipeContextFactory<TResult>(Type key)
        {
            var messageType = key.GetClosingArguments(typeof(Input<>)).Single();

            IPipeContextProviderFactory<Input, Type> factory = GetOrAdd(messageType);

            return factory.GetPipeContextFactory<TResult>(key);
        }

        public IPipeContextProviderFactory<Input, Type> GetOrAdd(Type type)
        {
            return _instance.GetOrAdd(type, _ =>
                (IPipeContextProviderFactory<Input, Type>)Activator.CreateInstance(typeof(CachedConfigurator<>).MakeGenericType(type)));
        }


        class CachedConfigurator<T> :
            IPipeContextProviderFactory<Input, Type>
            where T : class
        {
            IPipeContextProvider<Input, TResult> IPipeContextProviderFactory<Input, Type>.GetPipeContextFactory<TResult>(Type key)
            {
                return new ContextProvider<T>() as IPipeContextProvider<Input, TResult>;
            }
        }


        class ContextProvider<T> :
            IPipeContextProvider<Input, Input<T>>
            where T : class
        {
            public bool TryGetContext(Input context, out Input<T> result)
            {
                result = new Input<T>(context.Value as T);

                return true;
            }
        }
    }
}