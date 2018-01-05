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
namespace GreenPipes.Pipes
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;


    /// <summary>
    /// A pipe that caches an instance of T, which is then used to invoke the specified pipe
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CachePipe<T> :
        ICachePipe<T>
        where T : class, PipeContext
    {
        public Task Send(IPipe<T> pipe, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICachePipe<out T>
        where T : class, PipeContext
    {
        /// <summary>
        /// Send the T context to the specified pipe, supporting cancellation as requested
        /// </summary>
        /// <param name="pipe">The target pipe</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Send(IPipe<T> pipe, CancellationToken cancellationToken = default(CancellationToken));
    }
}