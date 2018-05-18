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
namespace GreenPipes
{
    using System;
    using System.Threading.Tasks;


    /// <summary>
    /// An initial context acquired to begin a retry filter
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public interface RetryPolicyContext<TContext> :
        IDisposable
        where TContext : class
    {
        /// <summary>
        /// The context being managed by the retry policy
        /// </summary>
        TContext Context { get; }

        /// <summary>
        /// Determines if the exception can be retried
        /// </summary>
        /// <param name="exception">The exception that occurred</param>
        /// <param name="retryContext">The retry context for the retry</param>
        /// <returns>True if the task should be retried</returns>
        bool CanRetry(Exception exception, out RetryContext<TContext> retryContext);

        /// <summary>
        /// Called after the retry attempt has failed
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        Task RetryFaulted(Exception exception);

        /// <summary>
        /// Cancel any pending or subsequent retries
        /// </summary>
        void Cancel();
    }
}