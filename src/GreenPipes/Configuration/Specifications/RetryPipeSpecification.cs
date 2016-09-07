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
    using System.Collections.Generic;
    using Configurators;
    using Filters;
    using Policies;


    public class RetryPipeSpecification<T> :
        IRetryConfigurator,
        IPipeSpecification<T>
        where T : class, PipeContext
    {
        IExceptionFilter _filter;
        RetryPolicyFactory _policyFactory;

        public RetryPipeSpecification()
        {
            _filter = new AllExceptionFilter();
        }

        public void Apply(IPipeBuilder<T> builder)
        {
            var retryPolicy = _policyFactory(_filter);

            builder.AddFilter(new RetryFilter<T>(retryPolicy));
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_filter == null)
                yield return this.Failure("ExceptionFilter", "must not be null");
            if (_policyFactory == null)
                yield return this.Failure("RetryPolicy", "must not be null");
        }

        public void SetRetryPolicy(RetryPolicyFactory factory)
        {
            _policyFactory = factory;
        }

        public void SetExceptionFilter(IExceptionFilter filter)
        {
            _filter = filter;
        }
    }
}