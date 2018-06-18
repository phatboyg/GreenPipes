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
namespace GreenPipes.Caching
{
    using System.Threading.Tasks;


    public interface INode<TValue>
        where TValue : class
    {
        /// <summary>
        /// The cached value
        /// </summary>
        Task<TValue> Value { get; }

        /// <summary>
        /// True if the node has a value, resolved, ready to rock
        /// </summary>
        bool HasValue { get; }

        /// <summary>
        /// True if the node value is invalid
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Get the node's value, passing a pending value if for some
        /// reason the node's value has not yet been accepted or has
        /// expired.
        /// </summary>
        /// <param name="pendingValue"></param>
        /// <returns></returns>
        Task<TValue> GetValue(IPendingValue<TValue> pendingValue);
    }
}