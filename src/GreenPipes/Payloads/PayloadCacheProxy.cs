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
    using System.Threading;


    /// <summary>
    /// A payload cache that delegates all calls to the <see cref="PipeContext"/> provided.
    /// Does not create a scope, all changes are applied directly to the provided context.
    /// </summary>
    public class PayloadCacheProxy :
        IPayloadCache,
        PipeContext
    {
        readonly PipeContext _context;

        public PayloadCacheProxy(PipeContext context)
        {
            _context = context;
            CancellationToken = context.CancellationToken;
        }

        public bool HasPayloadType(Type payloadType)
        {
            return _context.HasPayloadType(payloadType);
        }

        public bool TryGetPayload<T>(out T payload)
            where T : class
        {
            return _context.TryGetPayload(out payload);
        }

        public T GetOrAddPayload<T>(PayloadFactory<T> payloadFactory)
            where T : class
        {
            return _context.GetOrAddPayload(payloadFactory);
        }

        public T AddOrUpdatePayload<T>(PayloadFactory<T> addFactory, UpdatePayloadFactory<T> updateFactory)
            where T : class
        {
            return _context.AddOrUpdatePayload(addFactory, updateFactory);
        }

        public IPayloadCache CreateScope()
        {
            return new PayloadCacheScope(_context);
        }

        public CancellationToken CancellationToken { get; }
    }
}