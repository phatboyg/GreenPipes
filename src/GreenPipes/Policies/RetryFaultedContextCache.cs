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
namespace GreenPipes.Policies
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Internals.Extensions;


    public class RetryFaultedContextCache
    {
        readonly ConcurrentDictionary<Type, Lazy<IRetryContextFaulted>> _types =
            new ConcurrentDictionary<Type, Lazy<IRetryContextFaulted>>();

        IRetryContextFaulted this[Type type] => _types.GetOrAdd(type, CreateTypeConverter).Value;

        public static Task RetryFaulted(RetryContext context, Exception exception)
        {
            return Cached.Converters.Value[context.ContextType].RetryFaulted(context, exception);
        }

        static Lazy<IRetryContextFaulted> CreateTypeConverter(Type type)
        {
            return new Lazy<IRetryContextFaulted>(() => CreateConverter(type));
        }

        static IRetryContextFaulted CreateConverter(Type type)
        {
            var converterType = typeof(RetryContextFaulted<>).MakeGenericType(type);

            return (IRetryContextFaulted)Activator.CreateInstance(converterType);
        }


        interface IRetryContextFaulted
        {
            Task RetryFaulted(RetryContext context, Exception exception);
        }


        class RetryContextFaulted<T> :
            IRetryContextFaulted
            where T : class, PipeContext
        {
            Task IRetryContextFaulted.RetryFaulted(RetryContext context, Exception exception)
            {
                if (context == null)
                    throw new ArgumentNullException(nameof(context));

                if (context is RetryContext<T> retryContext)
                    return retryContext.RetryFaulted(exception);

                throw new ArgumentException($"The RetryContext was not an expected type: {TypeCache<T>.ShortName}");
            }
        }


        static class Cached
        {
            internal static readonly Lazy<RetryFaultedContextCache> Converters =
                new Lazy<RetryFaultedContextCache>(() => new RetryFaultedContextCache(), LazyThreadSafetyMode.PublicationOnly);
        }
    }
}