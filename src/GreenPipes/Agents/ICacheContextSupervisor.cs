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
namespace GreenPipes.Agents
{
    using System.Threading;
    using System.Threading.Tasks;


    public interface ICacheContextSupervisor<out TContext> :
        ISupervisor
        where TContext : class, PipeContext
    {
        /// <summary>
        /// Send the cached context through the pipe
        /// </summary>
        /// <param name="pipe"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Send(IPipe<TContext> pipe, CancellationToken cancellationToken = default(CancellationToken));
    }
}