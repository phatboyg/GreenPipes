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
    /// A factory which creates the actual and active contexts for a ContextAgent
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public interface IAgentContextFactory<TContext>
        where TContext : class, PipeContext
    {
        /// <summary>
        /// Create the agent context, which is the actual context, and not a copy of it
        /// </summary>
        /// <param name="agency">The agent containing the context</param>
        /// <returns></returns>
        AgentContextHandle<TContext> CreateContext(IAgency agency);

        /// <summary>
        /// Create an active agent context, which is a reference to the actual context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ActiveAgentContextHandle<TContext>> CreateActiveContext(AgentContextHandle<TContext> context,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}