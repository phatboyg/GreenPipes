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
    /// <summary>
    /// The response context
    /// </summary>
    public interface ResultContext :
        PipeContext
    {
        /// <summary>
        /// Returns the result type specified, if it is available
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetResult<T>()
            where T : class;

        /// <summary>
        /// Returns the result type specified if matched, otherwise returns false
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryGetResult<T>(out T result)
            where T : class;
    }


    public interface ResultContext<out TResult> :
        ResultContext
        where TResult : class
    {
        TResult Result { get; }
    }


    /// <summary>
    /// A response context combined a request with the applied response
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface ResultContext<out TRequest, out TResult> :
        ResultContext<TResult>
        where TRequest : class
        where TResult : class
    {
        TRequest Request { get; }
    }
}