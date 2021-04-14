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
namespace GreenPipes.BenchmarkConsole
{
    using System;


    /// <summary>
    /// Supports the reading of the property cache
    /// </summary>
    public interface IReadOnlyPayloadCollection
    {
        /// <summary>
        /// Checks if the property exists in the cache
        /// </summary>
        /// <param name="payloadType">The property type</param>
        /// <returns>True if the property exists in the cache, otherwise false</returns>
        bool HasPayloadType(Type payloadType);

        /// <summary>
        /// Returns the value of the property if it exists in the cache
        /// </summary>
        /// <typeparam name="TPayload">The property type</typeparam>
        /// <param name="payload">The property value</param>
        /// <returns>True if the value was returned, otherwise false</returns>
        bool TryGetPayload<TPayload>(out TPayload payload)
            where TPayload : class;
    }
}