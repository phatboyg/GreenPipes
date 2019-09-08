// Copyright 2012-2019 Chris Patterson
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
    using System.Reflection;
    using System.Threading;
    using Payloads;


    /// <summary>
    /// The base for a pipe context, with the underlying support for managing payloads (out-of-band data
    /// that is carried along with the context).
    /// </summary>
    public abstract class BasePipeContext
    {
        IPayloadCache _payloadCache;

        /// <summary>
        /// A pipe with no cancellation support
        /// </summary>
        protected BasePipeContext()
        {
            CancellationToken = CancellationToken.None;
        }

        /// <summary>
        /// A pipe using the specified <paramref name="cancellationToken"/>
        /// </summary>
        /// <param name="cancellationToken">A cancellation token</param>
        protected BasePipeContext(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// A pipe with no cancellation support, using the specified <paramref name="payloadCache"/>
        /// </summary>
        /// <param name="payloadCache"></param>
        protected BasePipeContext(IPayloadCache payloadCache)
        {
            _payloadCache = payloadCache ?? throw new ArgumentNullException(nameof(payloadCache));

            CancellationToken = CancellationToken.None;
        }

        /// <summary>
        /// A pipe using the specified <paramref name="cancellationToken"/> and <paramref name="payloadCache"/>
        /// </summary>
        /// <param name="payloadCache">A payload cache</param>
        /// <param name="cancellationToken">A cancellation token</param>
        protected BasePipeContext(IPayloadCache payloadCache, CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;

            _payloadCache = payloadCache;
        }

        /// <summary>
        /// Returns the CancellationToken for the context (implicit interface)
        /// </summary>
        public virtual CancellationToken CancellationToken { get; }

        /// <summary>
        /// Returns true if the payload type is included with or supported by the context type
        /// </summary>
        /// <param name="payloadType"></param>
        /// <returns></returns>
        public virtual bool HasPayloadType(Type payloadType)
        {
            return payloadType.GetTypeInfo().IsInstanceOfType(this) || PayloadCache.HasPayloadType(payloadType);
        }

        /// <summary>
        /// Attempts to get the specified payload type
        /// </summary>
        /// <param name="payload"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual bool TryGetPayload<T>(out T payload)
            where T : class
        {
            if (this is T context)
            {
                payload = context;
                return true;
            }

            return PayloadCache.TryGetPayload(out payload);
        }

        /// <summary>
        /// Get or add a payload to the context, using the provided payload factory.
        /// </summary>
        /// <param name="payloadFactory">The payload factory, which is only invoked if the payload is not present.</param>
        /// <typeparam name="T">The payload type</typeparam>
        /// <returns></returns>
        public virtual T GetOrAddPayload<T>(PayloadFactory<T> payloadFactory)
            where T : class
        {
            if (this is T context)
                return context;

            return PayloadCache.GetOrAddPayload(payloadFactory);
        }

        /// <summary>
        /// Either adds a new payload, or updates an existing payload
        /// </summary>
        /// <param name="addFactory">The payload factory called if the payload is not present</param>
        /// <param name="updateFactory">The payload factory called if the payload already exists</param>
        /// <typeparam name="T">The payload type</typeparam>
        /// <returns></returns>
        public virtual T AddOrUpdatePayload<T>(PayloadFactory<T> addFactory, UpdatePayloadFactory<T> updateFactory)
            where T : class
        {
            if (this is T context)
                return context;

            return PayloadCache.AddOrUpdatePayload(addFactory, updateFactory);
        }

        protected IPayloadCache PayloadCache
        {
            get
            {
                if (_payloadCache != null)
                    return _payloadCache;

                while (Volatile.Read(ref _payloadCache) == null)
                    Interlocked.CompareExchange(ref _payloadCache, new ListPayloadCache(), null);

                return _payloadCache;
            }
        }
    }
}