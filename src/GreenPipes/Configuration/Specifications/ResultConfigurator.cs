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
    using System.Collections.Generic;
    using Builders;
    using Configurators;
    using Pipes;


    public class ResultConfigurator<TRequest, TResult> :
        IResultConfigurator<TRequest, TResult>,
        IBuildResultPipeConfigurator<TRequest, TResult>,
        IBuildPipeConfigurator<ResultContext<TRequest, TResult>>

    {
        readonly IPipe<RequestContext> _pipe;
        readonly IBuildPipeConfigurator<ResultContext<TRequest, TResult>> _pipeConfigurator;

        public ResultConfigurator(IPipe<RequestContext> pipe)
        {
            _pipe = pipe;
            _pipeConfigurator = new PipeConfigurator<ResultContext<TRequest, TResult>>();
        }

        IPipe<ResultContext<TRequest, TResult>> IBuildPipeConfigurator<ResultContext<TRequest, TResult>>.Build()
        {
            return _pipeConfigurator.Build();
        }

        public IRequestPipe<TRequest, TResult> Build()
        {
            IPipe<ResultContext<TRequest, TResult>> responsePipe = _pipeConfigurator.Build();

            return new RequestPipe<TRequest, TResult>(_pipe, responsePipe);
        }

        public IEnumerable<ValidationResult> Validate()
        {
            return _pipeConfigurator.Validate();
        }

        public void AddPipeSpecification(IPipeSpecification<ResultContext<TRequest, TResult>> specification)
        {
            _pipeConfigurator.AddPipeSpecification(specification);
        }
    }
}