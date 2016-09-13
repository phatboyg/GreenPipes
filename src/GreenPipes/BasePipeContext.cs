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
    using Payloads;


    /// <summary>
    /// The base for a pipe context, with the underlying support for managing paylaods (out-of-band data
    /// that is carried along with the context).
    /// </summary>
    public abstract class BasePipeContext
    {
        readonly CancellationTokenSource _cancellationTokenSource;
        readonly IPayloadCache _payloadCache;

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
        /// A new pipe context with an existing payload cache -- includes a new CancellationTokenSource. If 
        /// cancellation is not supported, use the above constructor with CancellationToken.None to avoid
        /// creating a token source.
        /// </summary>
        /// <param name="payloadCache"></param>
        protected BasePipeContext(IPayloadCache payloadCache)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken = _cancellationTokenSource.Token;

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
        /// Returns the CancellationToken for the context (implicit interface)
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Returns true if the payload type is included with or supported by the context type
        /// </summary>
        /// <param name="payloadType"></param>
        /// <returns></returns>
        public virtual bool HasPayloadType(Type payloadType)
        {
            return payloadType.IsInstanceOfType(this) || _payloadCache.HasPayloadType(payloadType);
        }

        /// <summary>
        /// Attemts 
        /// </summary>
        /// <param name="payload"></param>
        /// <typeparam name="TPayload"></typeparam>
        /// <returns></returns>
        public virtual bool TryGetPayload<TPayload>(out TPayload payload)
            where TPayload : class
        {
            payload = this as TPayload;
            if (payload != null)
                return true;

            return _payloadCache.TryGetPayload(out payload);
        }

        /// <summary>
        /// Get or add a payload to the context, using the provided payload factory.
        /// </summary>
        /// <param name="payloadFactory">The payload factory, which is only invoked if the payload is not present.</param>
        /// <typeparam name="TPayload">The payload type</typeparam>
        /// <returns></returns>
        public virtual TPayload GetOrAddPayload<TPayload>(PayloadFactory<TPayload> payloadFactory)
            where TPayload : class
        {
            var context = this as TPayload;
            if (context != null)
                return context;

            return _payloadCache.GetOrAddPayload(payloadFactory);
        }

        /// <summary>
        /// Cancel the cancellation token
        /// </summary>
        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}