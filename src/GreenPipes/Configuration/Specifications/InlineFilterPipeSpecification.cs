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
    using Filters;


    /// <summary>
    /// Adds an arbitrary filter to the pipe
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class InlineFilterPipeSpecification<TContext> :
        IPipeSpecification<TContext>
        where TContext : class, PipeContext
    {
        readonly InlineFilterMethod<TContext> _filterMethod;

        public InlineFilterPipeSpecification(InlineFilterMethod<TContext> filterMethod)
        {
            _filterMethod = filterMethod;
        }

        public void Apply(IPipeBuilder<TContext> builder)
        {
            builder.AddFilter(new InlineFilter<TContext>(_filterMethod));
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (_filterMethod == null)
                yield return this.Failure("FilterMethod", "must not be null");
        }
    }
}