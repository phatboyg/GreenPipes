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
namespace GreenPipes.Payloads
{
    /// <summary>
    /// The context properties
    /// </summary>
    public interface IPayloadCache :
        IReadOnlyPayloadCollection
    {
        /// <summary>
        /// Return an existing or create a new property
        /// </summary>
        /// <param name="payloadFactory"></param>
        /// <returns></returns>
        T GetOrAddPayload<T>(PayloadFactory<T> payloadFactory)
            where T : class;

        /// <summary>
        /// Either adds a new payload, or updates an existing payload
        /// </summary>
        /// <param name="addFactory">The payload factory called if the payload is not present</param>
        /// <param name="updateFactory">The payload factory called if the payload already exists</param>
        /// <typeparam name="T">The payload type</typeparam>
        /// <returns></returns>
        T AddOrUpdatePayload<T>(PayloadFactory<T> addFactory, UpdatePayloadFactory<T> updateFactory)
            where T : class;

        /// <summary>
        /// Returns a scope from the current cache state
        /// </summary>
        /// <returns></returns>
        IPayloadCache CreateScope();
    }
}