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
namespace GreenPipes.Payloads
{
    using System;
    using System.Collections.Generic;


    public class ListPayloadCache :
        IPayloadCache
    {
        IList<object> _cache;

        public bool HasPayloadType(Type payloadType)
        {
            lock (this)
            {
                if (_cache == null)
                    return false;

                for (int i = _cache.Count - 1; i >= 0; i--)
                {
                    if (payloadType.IsInstanceOfType(_cache[i]))
                        return true;
                }
            }

            return false;
        }

        public bool TryGetPayload<TPayload>(out TPayload payload)
            where TPayload : class
        {
            if (_cache == null)
            {
                payload = default;
                return false;
            }

            lock (this)
            {
                for (int i = _cache.Count - 1; i >= 0; i--)
                {
                    if (_cache[i] is TPayload p)
                    {
                        payload = p;
                        return true;
                    }
                }
            }

            payload = default;
            return false;
        }

        public T GetOrAddPayload<T>(PayloadFactory<T> payloadFactory)
            where T : class
        {
            lock (this)
            {
                if (_cache != null)
                {
                    for (int i = _cache.Count - 1; i >= 0; i--)
                    {
                        if (_cache[i] is T result)
                            return result;
                    }
                }

                T payload = payloadFactory();

                if (_cache != null)
                    _cache.Add(payload);
                else
                    _cache = new List<object>(1) {payload};

                return payload;
            }
        }

        public T AddOrUpdatePayload<T>(PayloadFactory<T> addFactory, UpdatePayloadFactory<T> updateFactory)
            where T : class
        {
            lock (this)
            {
                if (_cache != null)
                {
                    for (int i = _cache.Count - 1; i >= 0; i--)
                    {
                        if (_cache[i] is T result)
                        {
                            var updated = updateFactory(result);

                            _cache[i] = updated;

                            return updated;
                        }
                    }
                }

                T payload = addFactory();

                if (_cache != null)
                    _cache.Add(payload);
                else
                    _cache = new List<object>(1) {payload};

                return payload;
            }
        }
    }
}