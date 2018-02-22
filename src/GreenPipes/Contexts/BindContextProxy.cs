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
namespace GreenPipes.Contexts
{
    using System;
    using System.Reflection;
    using System.Threading;


    /// <summary>
    /// The BindContext
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TSource"></typeparam>
    public class BindContextProxy<TContext, TSource> :
        BindContext<TContext, TSource>
        where TContext : class, PipeContext
        where TSource : class
    {
        readonly TContext _context;
        readonly TSource _sourceContext;

        public BindContextProxy(TContext context, TSource source)
        {
            _context = context;
            _sourceContext = source;
        }

        TContext BindContext<TContext, TSource>.Context => _context;

        TSource BindContext<TContext, TSource>.SourceContext => _sourceContext;

        CancellationToken PipeContext.CancellationToken => _context.CancellationToken;

        bool PipeContext.HasPayloadType(Type payloadType)
        {
            return payloadType.GetTypeInfo().IsInstanceOfType(_sourceContext) || _context.HasPayloadType(payloadType);
        }

        bool PipeContext.TryGetPayload<T>(out T payload)
        {
            payload = _sourceContext as T;
            if (payload != null)
                return true;

            return _context.TryGetPayload(out payload);
        }

        T PipeContext.GetOrAddPayload<T>(PayloadFactory<T> payloadFactory)
        {
            var context = _sourceContext as T;
            if (context != null)
                return context;

            return _context.GetOrAddPayload(payloadFactory);
        }

        T PipeContext.AddOrUpdatePayload<T>(PayloadFactory<T> addFactory, UpdatePayloadFactory<T> updateFactory)
        {
            // can't modify implicit payload types
            var context = _sourceContext as T;
            if (context != null)
                return context;

            return _context.AddOrUpdatePayload(addFactory, updateFactory);
        }
    }
}