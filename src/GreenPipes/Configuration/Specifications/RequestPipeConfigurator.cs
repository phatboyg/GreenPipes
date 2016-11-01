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
namespace GreenPipes.Specifications
{
    using System;
    using Builders;
    using Configurators;
    using Filters;
    using Routers;


    public class RequestPipeConfigurator<TRequest> :
        IRequestConfigurator<TRequest>
    {
        readonly IPipe<RequestContext> _pipe;
        readonly IBuildPipeConfigurator<RequestContext> _pipeConfigurator;
        DynamicFilter<ResultContext> _dynamicFilter;

        public RequestPipeConfigurator(IPipe<RequestContext> pipe)
        {
            _pipe = pipe;
            _pipeConfigurator = new PipeConfigurator<RequestContext>();
            _dynamicFilter = new DynamicFilter<ResultContext>(new ResponseConverterFactory());
        }

        public IRequestPipe<TRequest, TResult> Result<TResult>(Action<IResultConfigurator<TRequest, TResult>> configure = null)
        {
            var resultPipeConfigurator = new ResultConfigurator<TRequest, TResult>(_pipe);

            configure?.Invoke(resultPipeConfigurator);

            return resultPipeConfigurator.Build();
        }
    }
}