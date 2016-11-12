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
namespace GreenPipes
{
    using System;
    using Configurators;


    public static class RequestConfigurationExtensions
    {
        /// <summary>
        /// Creates a request pipe using an existing pipe
        /// </summary>
        /// <typeparam name="TRequest">The request type</typeparam>
        /// <param name="requestPipe">The pipe configurator</param>
        /// <param name="configureResults"></param>
        public static IRequestPipe<TRequest> CreateRequestPipe<TRequest>(this IPipe<RequestContext> requestPipe,
            params Func<IRequestConfigurator<TRequest>, IRequestPipe<TRequest>>[] configureResults)
            where TRequest : class
        {
            if (requestPipe == null)
                throw new ArgumentNullException(nameof(requestPipe));
            if (configureResults == null)
                throw new ArgumentNullException(nameof(configureResults));

            var requestConfigurator = new RequestConfigurator(requestPipe);

            return requestConfigurator.Request(configureResults);
        }

        /// <summary>
        /// Creates a request pipe using an existing pipe
        /// </summary>
        /// <typeparam name="TRequest">The request type</typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="requestPipe">The pipe configurator</param>
        /// <param name="configure"></param>
        public static IRequestPipe<TRequest, TResult> CreateRequestPipe<TRequest, TResult>(this IPipe<RequestContext> requestPipe,
            Action<IResultConfigurator<TRequest, TResult>> configure = null)
            where TRequest : class
            where TResult : class
        {
            if (requestPipe == null)
                throw new ArgumentNullException(nameof(requestPipe));

            var requestConfigurator = new RequestConfigurator(requestPipe);

            return requestConfigurator.Request(configure);
        }

        /// <summary>
        /// Handle the request on the dispatch pipeline
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="configure"></param>
        public static void Handle<TRequest>(this IDispatchConfigurator<RequestContext> configurator,
            Action<IPipeConfigurator<RequestContext<TRequest>>> configure = null)
            where TRequest : class
        {
            configurator.Pipe(configure);
        }
    }
}