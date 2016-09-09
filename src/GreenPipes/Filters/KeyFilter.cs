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
namespace GreenPipes.Filters
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;


    /// <summary>
    /// Handles the registration of requests and connecting them to the consume pipe
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class KeyFilter<T, TKey> :
        IFilter<T>,
        IPipeConnector<T, TKey>
        where T : class, PipeContext
    {
        readonly KeyAccessor<T, TKey> _keyAccessor;
        readonly ConcurrentDictionary<TKey, IPipe<T>> _pipes;

        public KeyFilter(KeyAccessor<T, TKey> keyAccessor)
        {
            _keyAccessor = keyAccessor;
            _pipes = new ConcurrentDictionary<TKey, IPipe<T>>();
        }

        public ConnectHandle ConnectPipe(TKey key, IPipe<T> pipe)
        {
            if (pipe == null)
                throw new ArgumentNullException(nameof(pipe));

            var added = _pipes.TryAdd(key, pipe);
            if (!added)
                throw new DuplicateKeyPipeConfigurationException($"A pipe with the specified key already exists: {key}");

            return new Handle(key, RemovePipe);
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateScope("key");

            ICollection<IPipe<T>> pipes = _pipes.Values;
            scope.Add("count", pipes.Count);

            foreach (IPipe<T> pipe in pipes)
                pipe.Probe(scope);
        }

        [DebuggerNonUserCode]
        public async Task Send(T context, IPipe<T> next)
        {
            var key = _keyAccessor(context);

            IPipe<T> pipe;
            if (_pipes.TryGetValue(key, out pipe))
                await pipe.Send(context).ConfigureAwait(false);

            await next.Send(context).ConfigureAwait(false);
        }

        void RemovePipe(TKey key)
        {
            IPipe<T> pipe;
            _pipes.TryRemove(key, out pipe);
        }


        class Handle :
            ConnectHandle
        {
            readonly TKey _key;
            readonly Action<TKey> _removeKey;

            public Handle(TKey key, Action<TKey> removeKey)
            {
                _key = key;
                _removeKey = removeKey;
            }

            public void Disconnect()
            {
                _removeKey(_key);
            }

            public void Dispose()
            {
                Disconnect();
            }
        }
    }
}