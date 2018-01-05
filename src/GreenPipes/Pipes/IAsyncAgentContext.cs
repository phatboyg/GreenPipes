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
    using System.Threading.Tasks;


    /// <summary>
    /// Used to set the agent context, or propogate faults
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public interface IAsyncAgentContext<TContext> :
        AgentContextHandle<TContext>
        where TContext : class, PipeContext
    {
        /// <summary>
        /// Called when the PipeContext has been created and is available for use.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task Created(TContext context);

        /// <summary>
        /// Called when the PipeContext creation failed
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        Task CreateFailed(Exception exception);

        /// <summary>
        /// Called when the successfully created PipeContext becomes faulted, indicating that it
        /// should no longer be used.
        /// </summary>
        /// <param name="exception">The exception which occurred</param>
        /// <returns></returns>
        Task Faulted(Exception exception);
    }
}