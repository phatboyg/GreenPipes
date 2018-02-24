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


    public interface RequestContext :
        PipeContext
    {
        /// <summary>
        /// True if the request has been completed and a result specified
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Attempt to specify a result for the request
        /// </summary>
        /// <typeparam name="T">The result type</typeparam>
        /// <param name="result">The result</param>
        /// <returns>True if the response was accepted, false if a response was already accepted</returns>
        bool TrySetResult<T>(T result)
            where T : class;

        /// <summary>
        /// Specify that the request faulted and will have an exception
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        bool TrySetException(Exception exception);

        /// <summary>
        /// Specify that the request was cancelled
        /// </summary>
        /// <returns></returns>
        bool TrySetCanceled();
    }


    /// <summary>
    /// The context of a request sent to a pipe
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public interface RequestContext<out TRequest> :
        RequestContext
        where TRequest : class
    {
        /// <summary>
        /// The request type that was sent to the pipe
        /// </summary>
        TRequest Request { get; }
    }
}