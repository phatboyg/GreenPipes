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
    using System.Threading;
    using System.Threading.Tasks;


    /// <summary>
    /// A context agent supports the creation of provocateurs
    /// </summary>
    public interface IAgency<TContext> :
        IAgency
        where TContext : class, PipeContext
    {
        /// <summary>
        /// Signaled when the context is ready, but not necessarily the provocateurs
        /// </summary>
        Task<ActiveAgentContextHandle<TContext>> GetActiveContext(CancellationToken cancellationToken = default(CancellationToken));
    }


    public interface IAgency :
        IAgent
    {
        /// <summary>
        /// Add an agent to the agency
        /// </summary>
        /// <param name="agent"></param>
        void Add(IAgent agent);

        /// <summary>
        /// Set the agency ready
        /// </summary>
        void SetReady();
    }
}