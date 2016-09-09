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
namespace GreenPipes.Specifications
{
    using System;
    using System.Collections.Generic;
    using Configurators;
    using Filters;


    public class DispatchPipeSpecification<TInput> :
        IPipeSpecification<TInput>,
        IDispatchConfigurator<TInput>
        where TInput : class, PipeContext
    {
        readonly IList<Action<IPipeConnector>> _connectActions;
        readonly IPipeContextConverterFactory<TInput> _pipeContextConverterFactory;

        public DispatchPipeSpecification(IPipeContextConverterFactory<TInput> pipeContextConverterFactory)
        {
            _pipeContextConverterFactory = pipeContextConverterFactory;

            _connectActions = new List<Action<IPipeConnector>>();
        }

        void IDispatchConfigurator<TInput>.Pipe<T>(Action<IPipeConfigurator<T>> configurePipe)
        {
            _connectActions.Add(connector =>
            {
                IPipe<T> pipe = Pipe.New(configurePipe);

                connector.ConnectPipe(pipe);
            });
        }

        public void Apply(IPipeBuilder<TInput> builder)
        {
            var dispatchFilter = new DynamicFilter<TInput>(_pipeContextConverterFactory);

            foreach (Action<IPipeConnector> action in _connectActions)
            {
                action(dispatchFilter);
            }

            builder.AddFilter(dispatchFilter);
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_pipeContextConverterFactory == null)
                yield return this.Failure("PipeContextProviderFactory", "must not be null");
        }
    }
}