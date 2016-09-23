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
    using System.Threading.Tasks;


    public interface IRetryObserver
    {
        /// <summary>
        /// Called before a message is dispatched to any consumers
        /// </summary>
        /// <param name="context">The consume context</param>
        /// <returns></returns>
        Task PostCreate<T>(RetryPolicyContext<T> context)
            where T : class, PipeContext;

        /// <summary>
        /// Called after a fault has occurred, but will be retried
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task PostFault<T>(RetryContext<T> context)
            where T : class, PipeContext;

        /// <summary>
        /// Called immediately before an exception will be retried
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task PreRetry<T>(RetryContext<T> context)
            where T : class, PipeContext;

        /// <summary>
        /// Called when the retry filter is no longer going to retry, and the context is faulted.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task RetryFault<T>(RetryContext<T> context)
            where T : class, PipeContext;
    }
}