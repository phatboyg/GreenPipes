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


    public class PipeContextPayloadAdapter :
        IPayloadCache
    {
        readonly PipeContext _context;

        public PipeContextPayloadAdapter(PipeContext context)
        {
            _context = context;
        }

        bool IReadOnlyPayloadCollection.HasPayloadType(Type payloadType)
        {
            return _context.HasPayloadType(payloadType);
        }

        bool IReadOnlyPayloadCollection.TryGetPayload<T>(out T payload)
        {
            return _context.TryGetPayload(out payload);
        }

        T IPayloadCache.GetOrAddPayload<T>(PayloadFactory<T> payloadFactory)
        {
            return _context.GetOrAddPayload(payloadFactory);
        }

        T IPayloadCache.AddOrUpdatePayload<T>(PayloadFactory<T> addFactory, UpdatePayloadFactory<T> updateFactory)
        {
            return _context.AddOrUpdatePayload(addFactory, updateFactory);
        }

        IPayloadCache IPayloadCache.CreateScope()
        {
            return new PayloadCacheScope(_context);
        }
    }
}