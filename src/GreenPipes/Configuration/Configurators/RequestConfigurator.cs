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
    using Specifications;


    /// <summary>
    /// Allows a request type to be specified on the pipe, and the subsequent configuration
    /// of the response types and response pipes.
    /// </summary>
    public class RequestConfigurator :
        IRequestConfigurator
    {
        readonly IPipe<RequestContext> _pipe;

        public RequestConfigurator(IPipe<RequestContext> pipe)
        {
            _pipe = pipe;
        }

        IBuildRequestPipeConfigurator<T> IRequestConfigurator.Request<T>(Action<IRequestConfigurator<T>> configureRequest)
        {
            var specification = new RequestPipeConfigurator<T>(_pipe);

            configureRequest?.Invoke(specification);

            throw new NotImplementedException();
        }

        public IBuildResultPipeConfigurator<TRequest, TResponse> Request<TRequest, TResponse>(
            Func<IResultConfigurator<TRequest, TResponse>, IBuildResultPipeConfigurator<TRequest, TResponse>> configureRequest = null)
        {
            var specification = new ResultConfigurator<TRequest, TResponse>(_pipe);

            configureRequest?.Invoke(specification);

            return specification;
        }
    }
}