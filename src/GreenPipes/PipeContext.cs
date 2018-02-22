// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace GreenPipes
{
    using System;
    using System.Threading;


    /// <summary>
    /// The base context for all pipe types, includes the payload sidebanding of data
    /// with the payload, as well as the cancellationToken to avoid passing it everywhere
    /// </summary>
    public interface PipeContext
    {
        /// <summary>
        /// Used to cancel the execution of the context
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Checks if a payload is present in the context
        /// </summary>
        /// <param name="payloadType"></param>
        /// <returns></returns>
        bool HasPayloadType(Type payloadType);

        /// <summary>
        /// Retrieves a payload from the pipe context
        /// </summary>
        /// <typeparam name="T">The payload type</typeparam>
        /// <param name="payload">The payload</param>
        /// <returns></returns>
        bool TryGetPayload<T>(out T payload)
            where T : class;

        /// <summary>
        /// Returns an existing payload or creates the payload using the factory method provided
        /// </summary>
        /// <typeparam name="T">The payload type</typeparam>
        /// <param name="payloadFactory">The payload factory is the payload is not present</param>
        /// <returns>The payload</returns>
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
    }
}