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
namespace GreenPipes
{
    using System;
    using System.Collections.Generic;
    using Specifications;


    public static class FilterConfigurationExtensions
    {
        /// <summary>
        /// Adds a filter to the pipe
        /// </summary>
        /// <typeparam name="T">The context type</typeparam>
        /// <param name="configurator">The pipe configurator</param>
        /// <param name="filter">The filter to add</param>
        public static void UseFilter<T>(this IPipeConfigurator<T> configurator, IFilter<T> filter)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var pipeBuilderConfigurator = new FilterPipeSpecification<T>(filter);

            configurator.AddPipeSpecification(pipeBuilderConfigurator);
        }

        /// <summary>
        /// Adds filters to the pipe
        /// </summary>
        /// <typeparam name="T">The context type</typeparam>
        /// <param name="configurator">The pipe configurator</param>
        /// <param name="filters">The filters to add</param>
        public static void UseFilters<T>(this IPipeConfigurator<T> configurator, IEnumerable<IFilter<T>> filters)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (filters == null)
                throw new ArgumentNullException(nameof(filters));

            foreach (IFilter<T> filter in filters)
            {
                var pipeBuilderConfigurator = new FilterPipeSpecification<T>(filter);

                configurator.AddPipeSpecification(pipeBuilderConfigurator);
            }
        }

        /// <summary>
        /// Adds filters to the pipe
        /// </summary>
        /// <typeparam name="T">The context type</typeparam>
        /// <param name="configurator">The pipe configurator</param>
        /// <param name="filters">The filters to add</param>
        public static void UseFilters<T>(this IPipeConfigurator<T> configurator, params IFilter<T>[] filters)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (filters == null)
                throw new ArgumentNullException(nameof(filters));

            foreach (IFilter<T> filter in filters)
            {
                var pipeBuilderConfigurator = new FilterPipeSpecification<T>(filter);

                configurator.AddPipeSpecification(pipeBuilderConfigurator);
            }
        }

        /// <summary>
        /// Adds a filter to the pipe which is of a different type than the native pipe context type
        /// </summary>
        /// <typeparam name="TContext">The context type</typeparam>
        /// <typeparam name="TFilter">The filter context type</typeparam>
        /// <param name="configurator">The pipe configurator</param>
        /// <param name="filter">The filter to add</param>
        /// <param name="contextProvider"></param>
        /// <param name="inputContextProvider"></param>
        public static void UseFilter<TContext, TFilter>(this IPipeConfigurator<TContext> configurator, IFilter<TFilter> filter,
            MergeFilterContextProvider<TContext, TFilter> contextProvider, FilterContextProvider<TFilter, TContext> inputContextProvider)
            where TContext : class, TFilter
            where TFilter : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var filterSpecification = new FilterPipeSpecification<TFilter>(filter);

            var pipeBuilderConfigurator = new SplitFilterPipeSpecification<TContext, TFilter>(filterSpecification, contextProvider, inputContextProvider);

            configurator.AddPipeSpecification(pipeBuilderConfigurator);
        }
    }
}