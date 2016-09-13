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
namespace GreenPipes.Payloads
{
    using System;
    using System.Threading;


    public class PayloadCacheScope :
        IPayloadCache
    {
        readonly IPayloadCache _parentCache;
        readonly IPayloadCache _payloadCache;

        public PayloadCacheScope(PipeContext context)
        {
            _parentCache = new PipeContextPayloadAdapter(context);

            CancellationToken = context.CancellationToken;

            _payloadCache = new PayloadCache();
        }

        public PayloadCacheScope(IPayloadCache parent, CancellationToken cancellationToken)
        {
            _parentCache = parent;

            CancellationToken = cancellationToken;

            _payloadCache = new PayloadCache();
        }

        public CancellationToken CancellationToken { get; }

        public bool HasPayloadType(Type payloadType)
        {
            return _payloadCache.HasPayloadType(payloadType) || _parentCache.HasPayloadType(payloadType);
        }

        public bool TryGetPayload<TPayload>(out TPayload payload)
            where TPayload : class
        {
            return _payloadCache.TryGetPayload(out payload) || _parentCache.TryGetPayload(out payload);
        }

        public TPayload GetOrAddPayload<TPayload>(PayloadFactory<TPayload> payloadFactory)
            where TPayload : class
        {
            TPayload payload;
            if (_payloadCache.TryGetPayload(out payload))
                return payload;

            if (_parentCache.TryGetPayload(out payload))
                return payload;

            return _payloadCache.GetOrAddPayload(payloadFactory);
        }

        public IPayloadCache CreateScope()
        {
            return new PayloadCacheScope(this, CancellationToken);
        }
    }
}