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


    public class DispatchPipeSpecification<TInput, TKey> :
        IPipeSpecification<TInput>,
        IDispatchConfigurator<TInput, TKey>
        where TInput : class, PipeContext
    {
        readonly IList<Action<IDispatchPipeConnector<TKey>>> _connectActions;
        readonly IPipeContextProviderFactory<TInput, TKey> _pipeContextProviderFactory;

        public DispatchPipeSpecification(IPipeContextProviderFactory<TInput, TKey> pipeContextProviderFactory)
        {
            _pipeContextProviderFactory = pipeContextProviderFactory;

            _connectActions = new List<Action<IDispatchPipeConnector<TKey>>>();
        }

        void IDispatchConfigurator<TInput, TKey>.Pipe<T>(TKey key, Action<IPipeConfigurator<T>> configurePipe)
        {
            _connectActions.Add(connector =>
            {
                IPipe<T> pipe = Pipe.New(configurePipe);

                connector.ConnectPipe(key, pipe);
            });
        }

        public void Apply(IPipeBuilder<TInput> builder)
        {
            var dispatchFilter = new DispatchFilter<TInput, TKey>(_pipeContextProviderFactory);

            foreach (var action in _connectActions)
            {
                action(dispatchFilter);
            }

            builder.AddFilter(dispatchFilter);
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_pipeContextProviderFactory == null)
                yield return this.Failure("PipeContextProviderFactory", "must not be null");
        }
    }
}