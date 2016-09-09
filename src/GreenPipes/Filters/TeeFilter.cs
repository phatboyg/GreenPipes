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
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Util;


    /// <summary>
    /// Connects multiple output pipes to a single input pipe
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TeeFilter<T> :
        IFilter<T>,
        IPipeConnector<T>
        where T : class, PipeContext
    {
        readonly Connectable<IPipe<T>> _connections;

        public TeeFilter()
        {
            _connections = new Connectable<IPipe<T>>();
        }

        public int Count => _connections.Count;

        void IProbeSite.Probe(ProbeContext context)
        {
            _connections.All(pipe =>
            {
                pipe.Probe(context);
                return true;
            });
        }

        [DebuggerNonUserCode]
        public async Task Send(T context, IPipe<T> next)
        {
            await _connections.ForEachAsync(async pipe => await pipe.Send(context).ConfigureAwait(false)).ConfigureAwait(false);

            await next.Send(context).ConfigureAwait(false);
        }

        public ConnectHandle ConnectPipe(IPipe<T> pipe)
        {
            return _connections.Connect(pipe);
        }
    }


    /// <summary>
    /// Connects multiple output pipes to a single input pipe
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey">The key type</typeparam>
    public class TeeFilter<T, TKey> :
        TeeFilter<T>,
        IPipeConnector<T, TKey>
        where T : class, PipeContext
    {
        readonly KeyAccessor<T, TKey> _keyAccessor;
        readonly Lazy<IPipeConnector<T, TKey>> _keyConnections;

        public TeeFilter(KeyAccessor<T, TKey> keyAccessor)
        {
            _keyAccessor = keyAccessor;

            _keyConnections = new Lazy<IPipeConnector<T, TKey>>(ConnectRequestFilter);
        }

        public ConnectHandle ConnectPipe(TKey key, IPipe<T> pipe)
        {
            return _keyConnections.Value.ConnectPipe(key, pipe);
        }

        IPipeConnector<T, TKey> ConnectRequestFilter()
        {
            var filter = new KeyFilter<T, TKey>(_keyAccessor);

            IPipe<T> pipe = Pipe.New<T>(x => x.UseFilter(filter));

            ConnectPipe(pipe);

            return filter;
        }
    }
}