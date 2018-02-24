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
    using Collections;
    using Internals.Extensions;


    public class PayloadCache :
        IPayloadCache
    {
        IPayloadCollection _collection;

        public PayloadCache()
        {
            _collection = EmptyPayloadCollection.Shared.Empty;
        }

        PayloadCache(IReadOnlyPayloadCollection collection)
        {
            _collection = new EmptyPayloadCollection(collection);
        }

        bool IReadOnlyPayloadCollection.HasPayloadType(Type payloadType)
        {
            return _collection.HasPayloadType(payloadType);
        }

        bool IReadOnlyPayloadCollection.TryGetPayload<T>(out T payload)
        {
            return _collection.TryGetPayload(out payload);
        }

        T IPayloadCache.GetOrAddPayload<T>(PayloadFactory<T> payloadFactory)
        {
            try
            {
                IPayloadValue<T> payload = null;

                IPayloadCollection currentCollection;
                do
                {
                    if (_collection.TryGetPayload(out T existingValue))
                        return existingValue;

                    IPayloadValue<T> contextProperty = payload ?? (payload = new PayloadValue<T>(payloadFactory()));

                    currentCollection = Volatile.Read(ref _collection);

                    Interlocked.CompareExchange(ref _collection, currentCollection.Add(contextProperty), currentCollection);
                }
                while (currentCollection == Volatile.Read(ref _collection));

                return payload.Value;
            }
            catch (Exception exception)
            {
                throw new PayloadFactoryException($"The payload factory faulted: {TypeCache<T>.ShortName}", exception);
            }
        }

        T IPayloadCache.AddOrUpdatePayload<T>(PayloadFactory<T> addFactory, UpdatePayloadFactory<T> updateFactory)
        {
            try
            {
                T previousValue = null;
                IPayloadValue<T> context = null;

                IPayloadCollection currentCollection;
                do
                {
                    if (_collection.TryGetPayload(out T existingValue))
                    {
                        if (context == null || previousValue != existingValue)
                            context = new PayloadValue<T>(updateFactory(existingValue));

                        previousValue = existingValue;

                        currentCollection = Volatile.Read(ref _collection);

                        Interlocked.CompareExchange(ref _collection, currentCollection.Add(context), currentCollection);
                    }
                    else
                    {
                        if (context == null)
                            context = new PayloadValue<T>(addFactory());

                        currentCollection = Volatile.Read(ref _collection);

                        Interlocked.CompareExchange(ref _collection, currentCollection.Add(context), currentCollection);
                    }
                }
                while (currentCollection == Volatile.Read(ref _collection));

                return context.Value;
            }
            catch (Exception exception)
            {
                throw new PayloadFactoryException($"The payload factory faulted: {TypeCache<T>.ShortName}", exception);
            }
        }

        IPayloadCache IPayloadCache.CreateScope()
        {
            return new PayloadCache(_collection);
        }
    }
}