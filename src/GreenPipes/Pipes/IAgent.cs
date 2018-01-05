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
    using Agents;


    public interface IAgent
    {
        /// <summary>
        /// The Task used to signal the agent is ready/faulted/cancelled
        /// </summary>
        Task Ready { get; }

        /// <summary>
        /// Completed when the agent is finished, no longer active
        /// </summary>
        Task Completed { get; }

        /// <summary>
        /// The token which indicates if the agent is in the process of stopping
        /// </summary>
        CancellationToken Stopping { get; }

        /// <summary>
        /// Stop the agent provocateur, signaling the agent is going to go away. This method can return immediately
        /// as the <see cref="Completed"/> property is used to ensure it's done.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task Stop(StopContext context);
    }
}