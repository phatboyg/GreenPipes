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
namespace GreenPipes
{
    using System;
    using System.Reflection;
    using System.Threading;
    using Payloads;


    /// <summary>
    /// The base for a pipe context, with the underlying support for managing paylaods (out-of-band data
    /// that is carried along with the context).
    /// </summary>
    public abstract class BasePipeContext
    {
        IPayloadCache _payloadCache;

        /// <summary>
        /// A new pipe context with an existing payload cache -- includes a new CancellationTokenSource. If 
        /// cancellation is not supported, use the above constructor with CancellationToken.None to avoid
        /// creating a token source.
        /// </summary>
        protected BasePipeContext()
        {
            CancellationToken = CancellationToken.None;
        }

        /// <summary>
        /// A new pipe context with an existing payload cache -- includes a new CancellationTokenSource. If
        /// cancellation is not supported, use the above constructor with CancellationToken.None to avoid
        /// creating a token source.
        /// </summary>
        protected BasePipeContext(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// A new pipe context with an existing payload cache -- includes a new CancellationTokenSource. If 
        /// cancellation is not supported, use the above constructor with CancellationToken.None to avoid
        /// creating a token source.
        /// </summary>
        /// <param name="payloadCache"></param>
        protected BasePipeContext(IPayloadCache payloadCache)
        {
            CancellationToken = CancellationToken.None;

            _payloadCache = payloadCache;
        }

        /// <summary>
        /// Uses the specified payloadCache and cancellationToken for the context
        /// </summary>
        /// <param name="payloadCache">A payload cache</param>
        /// <param name="cancellationToken">A cancellation token</param>
        protected BasePipeContext(IPayloadCache payloadCache, CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;

            _payloadCache = payloadCache;
        }

        /// <summary>
        /// A new pipe context based off an existing pipe context, which delegates the payloadCache
        /// to the existing pipe context.
        /// </summary>
        /// <param name="context"></param>
        protected BasePipeContext(PipeContext context)
            : this(new PayloadCacheProxy(context), context.CancellationToken)
        {
        }

        /// <summary>
        /// A new pipe context based off an existing pipe context, which delegates the payloadCache
        /// to the existing pipe context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        protected BasePipeContext(PipeContext context, CancellationToken cancellationToken)
            : this(new PayloadCacheProxy(context), cancellationToken)
        {
        }

        /// <summary>
        /// Returns the CancellationToken for the context (implicit interface)
        /// </summary>
        public virtual CancellationToken CancellationToken { get; }

        IPayloadCache PayloadCache
        {
            get
            {
                if (_payloadCache != null)
                    return _payloadCache;

                while (Volatile.Read(ref _payloadCache) == null)
                    Interlocked.CompareExchange(ref _payloadCache, new PayloadCache(), null);

                return _payloadCache;
            }
        }

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
        /// Attemts 
        /// </summary>
        /// <param name="payload"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual bool TryGetPayload<T>(out T payload)
            where T : class
        {
            payload = this as T;
            if (payload != null)
                return true;

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
    }
}