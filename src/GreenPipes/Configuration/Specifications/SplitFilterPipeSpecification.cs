// Copyright 2012-2018 Chris Patterson
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
    /// <typeparam name="TFilter">The filter type</typeparam>
    public class SplitFilterPipeSpecification<TContext, TFilter> :
        IPipeSpecification<TContext>
        where TContext : class, PipeContext
        where TFilter : class, PipeContext
    {
        readonly MergeFilterContextProvider<TContext, TFilter> _contextProvider;
        readonly FilterContextProvider<TFilter, TContext> _inputContextProvider;
        readonly IPipeSpecification<TFilter> _specification;

        public SplitFilterPipeSpecification(IPipeSpecification<TFilter> specification, MergeFilterContextProvider<TContext, TFilter> contextProvider,
            FilterContextProvider<TFilter, TContext> inputContextProvider)
        {
            _specification = specification;
            _contextProvider = contextProvider;
            _inputContextProvider = inputContextProvider;
        }

        public void Apply(IPipeBuilder<TContext> builder)
        {
            var splitBuilder = new Builder(builder, _contextProvider, _inputContextProvider);

            _specification.Apply(splitBuilder);
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_specification == null)
                yield return this.Failure("Specification", "must not be null");
            if (_contextProvider == null)
                yield return this.Failure("ContextProvider", "must not be null");
        }


        class Builder :
            IPipeBuilder<TFilter>
        {
            readonly IPipeBuilder<TContext> _builder;
            readonly MergeFilterContextProvider<TContext, TFilter> _contextProvider;
            readonly FilterContextProvider<TFilter, TContext> _inputContextProvider;

            public Builder(IPipeBuilder<TContext> builder, MergeFilterContextProvider<TContext, TFilter> contextProvider,
                FilterContextProvider<TFilter, TContext> inputContextProvider)
            {
                _builder = builder;
                _contextProvider = contextProvider;
                _inputContextProvider = inputContextProvider;
            }

            public void AddFilter(IFilter<TFilter> filter)
            {
                var splitFilter = new SplitFilter<TContext, TFilter>(filter, _contextProvider, _inputContextProvider);

                _builder.AddFilter(splitFilter);
            }
        }
    }
}