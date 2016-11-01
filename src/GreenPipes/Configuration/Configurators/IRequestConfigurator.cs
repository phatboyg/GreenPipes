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
namespace GreenPipes.Configurators
{
    using System;
    using Builders;


    /// <summary>
    /// Configure a request, specifying the responses and their pipes
    /// </summary>
    public interface IRequestConfigurator
    {
        IBuildRequestPipeConfigurator<TRequest> Request<TRequest>(Action<IRequestConfigurator<TRequest>> configureRequest);

        /// <summary>
        /// Create a pipe that handles a request with a single response
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="configureRequest"></param>
        /// <returns></returns>
        IBuildResultPipeConfigurator<TRequest, TResponse> Request<TRequest, TResponse>(
            Func<IResultConfigurator<TRequest, TResponse>, IBuildResultPipeConfigurator<TRequest, TResponse>> configureRequest = null);
    }


    /// <summary>
    /// Configure a request, specifying the responses and their pipes
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public interface IRequestConfigurator<TRequest>
    {
        /// <summary>
        /// Declares a result for the request which can be set by a service
        /// </summary>
        /// <typeparam name="TResult">The result type</typeparam>
        /// <param name="configure">Configure the result pipe</param>
        /// <returns></returns>
        IRequestPipe<TRequest, TResult> Result<TResult>(Action<IResultConfigurator<TRequest, TResult>> configure = null);
    }
}