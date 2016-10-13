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
    using Filters;


    /// <summary>
    /// Configures a concurrency limit on the pipe. If the management endpoint is specified,
    /// the consumer and appropriate mediator is created to handle the adjustment of the limit.
    /// </summary>
    /// <typeparam name="T">The message type being limited</typeparam>
    public class ConcurrencyLimitPipeSpecification<T> :
        IPipeSpecification<T>
        where T : class, PipeContext
    {
        readonly int _concurrencyLimit;

        readonly IPipeRouter _router;

        public ConcurrencyLimitPipeSpecification(int concurrencyLimit, IPipeRouter router = null)
        {
            _concurrencyLimit = concurrencyLimit;

            _router = router;
        }

        public void Apply(IPipeBuilder<T> builder)
        {
            var filter = new ConcurrencyLimitFilter<T>(_concurrencyLimit);

            builder.AddFilter(filter);

            _router?.ConnectPipe(filter);
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_concurrencyLimit < 1)
                yield return this.Failure("ConcurrencyLimit", "must be >= 1");
        }
    }
}