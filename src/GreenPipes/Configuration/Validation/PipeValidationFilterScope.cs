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
namespace GreenPipes.Validation
{
    using System;
    using System.Collections.Generic;


    public class PipeValidationFilterScope<TContext> :
        ValidationFilterScope
        where TContext : class, PipeContext
    {
        readonly PipeValidationContext _context;
        readonly IPipeSpecification<TContext> _specification;
        readonly Type _filterType;

        public PipeValidationFilterScope(PipeValidationContext context, IPipeSpecification<TContext> specification, Type filterType)
        {
            _context = context;
            _specification = specification;
            _filterType = filterType;
        }

        public IEnumerable<ValidationResult> ProvidesPayload<T>() where T : class
        {
            return _context.ProvidesPayload<T>(new ProviderInfo
            {
                PayloadType = typeof(T),
                FilterType = _filterType,
                ContextType = typeof(TContext)
            });
        }

        IEnumerable<ValidationResult> ValidationFilterScope.RequiresPayload<T>()
        {
            return _context.RequiresPayload<T>();
        }

        ValidationFilterScope ValidationContext.CreateFilterScope<T>(IPipeSpecification<T> specification, Type filterType)
        {
            return _context.CreateFilterScope(specification, filterType);
        }


        class ProviderInfo :
            PayloadProviderInfo
        {
            public Type PayloadType { get; set; }

            public Type FilterType { get; set; }

            public Type ContextType { get; set; }
        }
    }
}