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
namespace GreenPipes
{
    using System;
    using System.Threading.Tasks;


    public interface RetryContext
    {
        /// <summary>
        /// The exception that originally caused the retry to be initiated
        /// </summary>
        Exception Exception { get; }

        /// <summary>
        /// The number of retries which were attempted beyond the initial attempt
        /// </summary>
        int RetryCount { get; }

        /// <summary>
        /// The time to wait before the next retry attempt
        /// </summary>
        TimeSpan? Delay { get; }

        /// <summary>
        /// Determines if the exception can be retried per the retry policy
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="retryContext">The retry context for the retry</param>
        /// <returns></returns>
        Task<bool> CanRetry(Exception exception, out RetryContext retryContext);
    }
}