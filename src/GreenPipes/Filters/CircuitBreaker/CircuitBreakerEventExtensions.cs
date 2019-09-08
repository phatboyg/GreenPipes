// Copyright 2012-2018 Chris Patterson
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
namespace GreenPipes.Filters.CircuitBreaker
{
    using System;
    using System.Threading.Tasks;
    using Contracts;


    public static class CircuitBreakerEventExtensions
    {
        /// <summary>
        /// Set the concurrency limit of the filter
        /// </summary>
        /// <param name="pipe"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static Task PublishCircuitBreakerOpened(this IPipe<EventContext> pipe, Exception exception)
        {
            return pipe.PublishEvent<CircuitBreakerOpened>(new Opened(exception));
        }

        /// <summary>
        /// Set the concurrency limit of the filter
        /// </summary>
        /// <param name="pipe"></param>
        /// <returns></returns>
        public static Task PublishCircuitBreakerClosed(this IPipe<EventContext> pipe)
        {
            return pipe.PublishEvent<CircuitBreakerClosed>(new Closed());
        }


        class Closed :
            CircuitBreakerClosed
        {
        }


        class Opened :
            CircuitBreakerOpened
        {
            public Opened(Exception exception)
            {
                Exception = exception;
            }

            public Exception Exception { get; }
        }
    }
}