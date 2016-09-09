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


    public class RescuePipeSpecification<TContext, TRescue> :
        ExceptionSpecification,
        IPipeSpecification<TContext>,
        IRescueConfigurator
        where TContext : class, PipeContext
        where TRescue : class, PipeContext
    {
        readonly RescueContextFactory<TContext, TRescue> _rescueContextFactory;
        readonly IPipe<TRescue> _rescuePipe;

        public RescuePipeSpecification(IPipe<TRescue> rescuePipe, RescueContextFactory<TContext, TRescue> rescueContextFactory)
        {
            _rescuePipe = rescuePipe;
            _rescueContextFactory = rescueContextFactory;
        }

        public void Apply(IPipeBuilder<TContext> builder)
        {
            builder.AddFilter(new RescueFilter<TContext, TRescue>(_rescuePipe, Filter, _rescueContextFactory));
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_rescuePipe == null)
                yield return this.Failure("RescuePipe", "must not be null");
            if (_rescueContextFactory == null)
                yield return this.Failure("RescueContextFactory", "must not be null");
        }
    }
}