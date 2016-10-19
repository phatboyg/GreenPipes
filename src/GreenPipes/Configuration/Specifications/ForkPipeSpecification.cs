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
    /// Adds a fork to the pipe
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class ForkPipeSpecification<TContext> :
        IPipeSpecification<TContext>
        where TContext : class, PipeContext
    {
        readonly IPipe<TContext> _pipe;

        public ForkPipeSpecification(IPipe<TContext> pipe)
        {
            _pipe = pipe;
        }

        public void Apply(IPipeBuilder<TContext> builder)
        {
            builder.AddFilter(new ForkFilter<TContext>(_pipe));
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (_pipe == null)
                yield return this.Failure("Pipe", "must not be null");
        }
    }
}