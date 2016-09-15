// Copyright 2013-2016 Chris Patterson
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
    /// Adds an arbitrary filter to the pipe
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="T">The filter type</typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class ResultFilterPipeSpecification<TContext, TResult> :
        IPipeSpecification<TContext, TResult>
        where TContext : class, PipeContext
        where TResult : class
    {
        readonly IPipeSpecification<TContext> _specification;

        public ResultFilterPipeSpecification(IPipeSpecification<TContext> specification)
        {
            _specification = specification;
        }

        public void Apply(IPipeBuilder<TContext, TResult> builder)
        {
            var splitBuilder = new Builder(builder);

            _specification.Apply(splitBuilder);
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_specification == null)
                yield return this.Failure("Specification", "must not be null");
        }


        class Builder :
            IPipeBuilder<TContext>
        {
            readonly IPipeBuilder<TContext, TResult> _builder;

            public Builder(IPipeBuilder<TContext, TResult> builder)
            {
                _builder = builder;
            }

            public void AddFilter(IFilter<TContext> filter)
            {
                var resultFilter = new ResultFilter<TContext, TResult>(filter);

                _builder.AddFilter(resultFilter);
            }
        }
    }
}