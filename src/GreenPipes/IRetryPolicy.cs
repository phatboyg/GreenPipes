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


    /// <summary>
    /// A retry policy determines how exceptions are handled, and whether or not the
    /// remaining filters should be retried
    /// </summary>
    public interface IRetryPolicy :
        IProbeSite
    {
        /// <summary>
        /// Creates a retry policy context for the retry, which initiates the exception tracking
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        RetryPolicyContext<T> CreatePolicyContext<T>(T context)
            where T : class, PipeContext;

        /// <summary>
        /// If the retry policy handles the exception, should return true
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        bool IsHandled(Exception exception);
    }
}