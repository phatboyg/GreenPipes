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
    /// Adds an arbitrary filter to the pipe
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class FilterPipeSpecification<TContext> :
        IPipeSpecification<TContext>
        where TContext : class, PipeContext
    {
        readonly IFilter<TContext> _filter;

        public FilterPipeSpecification(IFilter<TContext> filter)
        {
            _filter = filter;
        }

        public void Apply(IPipeBuilder<TContext> builder)
        {
            builder.AddFilter(_filter);
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_filter == null)
                yield return this.Failure("Filter", "must not be null");
        }
    }


    /// <summary>
    /// Adds an arbitrary filter to the pipe
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TFilter">The filter type</typeparam>
    public class FilterPipeSpecification<TContext, TFilter> :
        IPipeSpecification<TContext>
        where TContext : class, TFilter
        where TFilter : class, PipeContext
    {
        readonly MergeFilterContextProvider<TContext, TFilter> _contextProvider;
        readonly IFilter<TFilter> _filter;
        readonly FilterContextProvider<TFilter, TContext> _filterContextProvider;

        public FilterPipeSpecification(IFilter<TFilter> filter, MergeFilterContextProvider<TContext, TFilter> contextProvider,
            FilterContextProvider<TFilter, TContext> filterContextProvider)
        {
            _filter = filter;
            _contextProvider = contextProvider;
            _filterContextProvider = filterContextProvider;
        }

        public void Apply(IPipeBuilder<TContext> builder)
        {
            var splitFilter = new SplitFilter<TContext, TFilter>(_filter, _contextProvider, _filterContextProvider);

            builder.AddFilter(splitFilter);
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_filter == null)
                yield return this.Failure("Filter", "must not be null");
            if (_contextProvider == null)
                yield return this.Failure("ContextProvider", "must not be null");
        }
    }
}