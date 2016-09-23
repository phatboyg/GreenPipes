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
    using Configurators;
    using Filters;


    public class RetryPipeSpecification<TContext> :
        ExceptionSpecification,
        IRetryConfigurator,
        IPipeSpecification<TContext>
        where TContext : class, PipeContext
    {
        readonly RetryObservable _observers;
        RetryPolicyFactory _policyFactory;

        public RetryPipeSpecification()
        {
            _observers = new RetryObservable();
        }

        public void Apply(IPipeBuilder<TContext> builder)
        {
            var retryPolicy = _policyFactory(Filter);

            builder.AddFilter(new RetryFilter<TContext>(retryPolicy, _observers));
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_policyFactory == null)
                yield return this.Failure("RetryPolicy", "must not be null");
        }

        public void SetRetryPolicy(RetryPolicyFactory factory)
        {
            _policyFactory = factory;
        }

        ConnectHandle IRetryObserverConnector.ConnectRetryObserver(IRetryObserver observer)
        {
            return _observers.Connect(observer);
        }
    }
}