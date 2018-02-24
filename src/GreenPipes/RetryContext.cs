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


    public interface RetryContext
    {
        /// <summary>
        /// The retry attempt currently being attempted
        /// </summary>
        int RetryAttempt { get; }

        /// <summary>
        /// The context type of the retry context
        /// </summary>
        Type ContextType { get; }
    }


    public interface RetryContext<TContext> :
        RetryPolicyContext<TContext>,
        RetryContext
        where TContext : class
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
        /// Called before the retry attempt is performed
        /// </summary>
        /// <returns></returns>
        Task PreRetry();
    }
}