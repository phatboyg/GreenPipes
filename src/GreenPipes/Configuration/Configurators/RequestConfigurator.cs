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
namespace GreenPipes.Configurators
{
    using System;
    using System.Collections.Generic;
    using Builders;
    using Contexts;
    using Pipes;
    using Validation;


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

        public IRequestPipe<TRequest> Request<TRequest>(params Func<IRequestConfigurator<TRequest>, IRequestPipe<TRequest>>[] configure)
            where TRequest : class
        {
            var specification = new RequestConfigurator<TRequest>(_pipe);

            foreach (Func<IRequestConfigurator<TRequest>, IRequestPipe<TRequest>> callback in configure)
                callback(specification);

            return specification.Build();
        }

        public IRequestPipe<TRequest, TResult> Request<TRequest, TResult>(Action<IResultConfigurator<TRequest, TResult>> configureRequest = null)
            where TRequest : class
            where TResult : class
        {
            var configurator = new ResultConfigurator<TRequest, TResult>(_pipe);

            configureRequest?.Invoke(configurator);

            return configurator.Build();
        }
    }


    public class RequestConfigurator<TRequest> :
        IRequestConfigurator<TRequest>
        where TRequest : class
    {
        readonly IPipe<RequestContext> _pipe;
        readonly IBuildPipeConfigurator<ResultContext> _resultPipeConfigurator;
        readonly DynamicRouter<ResultContext> _router;

        public RequestConfigurator(IPipe<RequestContext> pipe)
        {
            _pipe = pipe;
            _resultPipeConfigurator = new PipeConfigurator<ResultContext>();
            _router = new DynamicRouter<ResultContext>(new ResultConverterFactory());
        }

        IRequestPipe<TRequest> IRequestConfigurator<TRequest>.Result<TResult>(Action<IRequestConfigurator<TRequest, TResult>> configure)
        {
            var requestConfigurator = new RequestConfigurator<TRequest, TResult>(_pipe);

            configure?.Invoke(requestConfigurator);

            IPipeConfigurationResult result = new PipeConfigurationResult(requestConfigurator.Validate());
            if (result.ContainsFailure)
                throw new PipeConfigurationException(result.GetMessage("The result configuration was invalid"));

            requestConfigurator.Connect(_router);

            return requestConfigurator.Build(_resultPipeConfigurator.Build());
        }

        void IPipeConfigurator<ResultContext>.AddPipeSpecification(IPipeSpecification<ResultContext> specification)
        {
            _resultPipeConfigurator.AddPipeSpecification(specification);
        }

        public IRequestPipe<TRequest> Build()
        {
            return new MultipleResultRequestPipe<TRequest>(_pipe, _resultPipeConfigurator.Build());
        }
    }


    /// <summary>
    /// This will become a specification, since the goal is to have everything rally around
    /// the dispatch pipe
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class RequestConfigurator<TRequest, TResult> :
        IBuildRequestPipeConfigurator<TRequest, TResult>
        where TRequest : class
        where TResult : class
    {
        readonly IPipe<RequestContext> _pipe;
        readonly IBuildPipeConfigurator<ResultContext<TRequest, TResult>> _pipeConfigurator;

        public RequestConfigurator(IPipe<RequestContext> pipe)
        {
            _pipe = pipe;
            _pipeConfigurator = new PipeConfigurator<ResultContext<TRequest, TResult>>();
        }

        public IRequestPipe<TRequest> Build(IPipe<ResultContext> resultPipe)
        {
            return new MultipleResultRequestPipe<TRequest>(_pipe, resultPipe);
        }

        public IEnumerable<ValidationResult> Validate()
        {
            return _pipeConfigurator.Validate();
        }

        public void Connect(IPipeConnector connector)
        {
            IPipe<ResultContext<TRequest, TResult>> responsePipe = _pipeConfigurator.Build();

            connector.ConnectPipe(responsePipe);
        }

        public void AddPipeSpecification(IPipeSpecification<ResultContext<TRequest, TResult>> specification)
        {
            _pipeConfigurator.AddPipeSpecification(specification);
        }
    }
}