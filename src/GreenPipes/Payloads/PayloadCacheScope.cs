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
    using System;


    public class PayloadCacheScope :
        IPayloadCache
    {
        readonly IPayloadCache _parentCache;
        readonly IPayloadCache _payloadCache;

        public PayloadCacheScope(PipeContext context)
        {
            _parentCache = new PipeContextPayloadAdapter(context);

            _payloadCache = new PayloadCache();
        }

        PayloadCacheScope(IPayloadCache parent)
        {
            _parentCache = parent;

            _payloadCache = new PayloadCache();
        }

        bool IReadOnlyPayloadCollection.HasPayloadType(Type payloadType)
        {
            return _payloadCache.HasPayloadType(payloadType) || _parentCache.HasPayloadType(payloadType);
        }

        bool IReadOnlyPayloadCollection.TryGetPayload<T>(out T payload)
        {
            return _payloadCache.TryGetPayload(out payload) || _parentCache.TryGetPayload(out payload);
        }

        T IPayloadCache.GetOrAddPayload<T>(PayloadFactory<T> payloadFactory)
        {
            if (_payloadCache.TryGetPayload(out T payload))
                return payload;

            if (_parentCache.TryGetPayload(out payload))
                return payload;

            return _payloadCache.GetOrAddPayload(payloadFactory);
        }

        T IPayloadCache.AddOrUpdatePayload<T>(PayloadFactory<T> addFactory, UpdatePayloadFactory<T> updateFactory)
        {
            if (_payloadCache.TryGetPayload(out T existingPayload))
                return _payloadCache.AddOrUpdatePayload(addFactory, updateFactory);

            if (_parentCache.TryGetPayload(out existingPayload))
            {
                T Add() => updateFactory(existingPayload);

                return _payloadCache.AddOrUpdatePayload(Add, updateFactory);
            }

            return _payloadCache.AddOrUpdatePayload(addFactory, updateFactory);
        }

        IPayloadCache IPayloadCache.CreateScope()
        {
            return new PayloadCacheScope(this);
        }
    }
}