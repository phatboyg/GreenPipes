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
namespace GreenPipes.Filters
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Util;


    /// <summary>
    /// Connects multiple output pipes to a single input pipe
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class TeeFilter<TContext> :
        ITeeFilter<TContext>
        where TContext : class, PipeContext
    {
        readonly Connectable<IPipe<TContext>> _connections;

        public TeeFilter()
        {
            _connections = new Connectable<IPipe<TContext>>();
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
        public async Task Send(TContext context, IPipe<TContext> next)
        {
            await _connections.ForEachAsync(pipe => pipe.Send(context)).ConfigureAwait(false);

            await next.Send(context).ConfigureAwait(false);
        }

        public ConnectHandle ConnectPipe(IPipe<TContext> pipe)
        {
            return _connections.Connect(pipe);
        }
    }


    /// <summary>
    /// Connects multiple output pipes to a single input pipe
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TKey">The key type</typeparam>
    public class TeeFilter<TContext, TKey> :
        TeeFilter<TContext>,
        ITeeFilter<TContext, TKey>
        where TContext : class, PipeContext
    {
        readonly KeyAccessor<TContext, TKey> _keyAccessor;
        readonly Lazy<IKeyPipeConnector<TKey>> _keyConnections;

        public TeeFilter(KeyAccessor<TContext, TKey> keyAccessor)
        {
            _keyAccessor = keyAccessor;

            _keyConnections = new Lazy<IKeyPipeConnector<TKey>>(ConnectKeyFilter);
        }

        public ConnectHandle ConnectPipe<T>(TKey key, IPipe<T> pipe)
            where T : class, PipeContext
        {
            return _keyConnections.Value.ConnectPipe(key, pipe);
        }

        IKeyPipeConnector<TKey> ConnectKeyFilter()
        {
            var filter = new KeyFilter<TContext, TKey>(_keyAccessor);

            IPipe<TContext> pipe = Pipe.New<TContext>(x => x.UseFilter(filter));

            ConnectPipe(pipe);

            return filter;
        }
    }
}