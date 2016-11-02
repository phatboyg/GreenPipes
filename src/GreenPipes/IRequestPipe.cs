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


    public interface IRequestPipe<in TRequest> :
        IProbeSite
        where TRequest : class
    {
        /// <summary>
        /// Send a request to the pipe
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<ResultContext> Send(TRequest context);
    }


    /// <summary>
    /// A request pipe which allows awaiting a specific response
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface IRequestPipe<in TRequest, TResult> :
        IProbeSite
        where TRequest : class
        where TResult : class
    {
        /// <summary>
        /// Send a request to the pipe
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ResultContext<TResult>> Send(TRequest request);
    }
}