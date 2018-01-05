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
    using System.Threading.Tasks;


    /// <summary>
    /// A handle to a PipeContext instance (of type <typeparam name="T">T</typeparam>), which can be discarded
    /// once it is no longer needed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface AgentContextHandle<T>
        where T : class, PipeContext
    {
        /// <summary>
        /// True if the context is valid and can be used
        /// </summary>
        bool IsInactive { get; }

        /// <summary>
        /// The PipeContext
        /// </summary>
        Task<T> Context { get; }

        /// <summary>
        /// Deactivates the context, making it unavailable for use
        /// </summary>
        /// <returns></returns>
        Task Disavow();
    }
}