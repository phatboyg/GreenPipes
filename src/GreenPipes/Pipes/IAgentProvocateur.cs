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
    /// <summary>
    /// An agent, secret, stands alone in the field of battle. Or is it, something else entirely?
    /// </summary>
    public interface IAgentProvocateur : 
        IAgent
    {
        /// <summary>
        /// Set the agent ready
        /// </summary>
        void SetReady();

        /// <summary>
        /// Set the agent complete
        /// </summary>
        void SetCompleted();
    }
}