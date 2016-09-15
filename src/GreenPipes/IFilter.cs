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
namespace GreenPipes
{
    using System.Threading.Tasks;


    /// <summary>
    /// A filter is a functional node in a pipeline, connected by pipes to
    /// other filters.
    /// </summary>
    /// <typeparam name="TContext">The pipe context type</typeparam>
    public interface IFilter<TContext> :
        IProbeSite
        where TContext : class, PipeContext
    {
        /// <summary>
        /// Sends a context to a filter, such that it can be processed and then passed to the
        /// specified output pipe for further processing.
        /// </summary>
        /// <param name="context">The pipe context type</param>
        /// <param name="next">The next pipe in the pipeline</param>
        /// <returns>An awaitable Task</returns>
        Task Send(TContext context, IPipe<TContext> next);
    }


    public interface IFilter<TContext, TResult> :
        IProbeSite
        where TContext : class, PipeContext
        where TResult : class
    {
        /// <summary>
        /// Sends a context to a filter, such that it can be processed and then passed to the
        /// specified output pipe for further processing.
        /// </summary>
        /// <param name="context">The pipe context type</param>
        /// <param name="next">The next pipe in the pipeline</param>
        /// <returns>An awaitable Task</returns>
        Task<TResult> Send(TContext context, IPipe<TContext, TResult> next);
    }
}