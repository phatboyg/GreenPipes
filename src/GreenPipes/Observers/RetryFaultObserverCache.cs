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
namespace GreenPipes.Observers
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;


    public class RetryFaultObserverCache
    {
        readonly ConcurrentDictionary<Type, Lazy<IRetryFaultObserver>> _types =
            new ConcurrentDictionary<Type, Lazy<IRetryFaultObserver>>();

        IRetryFaultObserver this[Type type] => _types.GetOrAdd(type, CreateTypeConverter).Value;

        public static Task RetryFault(IRetryObserver observer, RetryContext context, Type contextType,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Cached.Converters.Value[contextType].RetryFault(observer, context);
        }

        static Lazy<IRetryFaultObserver> CreateTypeConverter(Type type)
        {
            return new Lazy<IRetryFaultObserver>(() => CreateConverter(type));
        }

        static IRetryFaultObserver CreateConverter(Type type)
        {
            var converterType = typeof(RetryFaultObserver<>).MakeGenericType(type);

            return (IRetryFaultObserver)Activator.CreateInstance(converterType);
        }


        interface IRetryFaultObserver
        {
            Task RetryFault(IRetryObserver observer, RetryContext context);
        }


        class RetryFaultObserver<T> :
            IRetryFaultObserver
            where T : class, PipeContext
        {
            public Task RetryFault(IRetryObserver observer, RetryContext context)
            {
                if (context == null)
                    throw new ArgumentNullException(nameof(context));

                var retryContext = context as RetryContext<T>;

                return observer.RetryFault(retryContext);
            }
        }


        static class Cached
        {
            internal static readonly Lazy<RetryFaultObserverCache> Converters =
                new Lazy<RetryFaultObserverCache>(() => new RetryFaultObserverCache(), LazyThreadSafetyMode.PublicationOnly);
        }
    }
}