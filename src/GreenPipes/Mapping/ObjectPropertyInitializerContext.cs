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
namespace GreenPipes.Mapping
{
    using System;
    using System.Threading;
    using Internals.Reflection;


    public readonly struct ObjectPropertyInitializerContext<T, TProperty> :
        PropertyInitializerContext<T, TProperty>
        where T : class
    {
        readonly InitalizerContext<T> _context;
        readonly IWriteProperty<T, TProperty> _writeProperty;

        public ObjectPropertyInitializerContext(InitalizerContext<T> context, IWriteProperty<T, TProperty> writeProperty)
        {
            _context = context;
            _writeProperty = writeProperty;
        }

        CancellationToken PipeContext.CancellationToken => _context.CancellationToken;

        bool PipeContext.HasPayloadType(Type payloadType)
        {
            return _context.HasPayloadType(payloadType);
        }

        bool PipeContext.TryGetPayload<T1>(out T1 payload)
        {
            return _context.TryGetPayload(out payload);
        }

        T1 PipeContext.GetOrAddPayload<T1>(PayloadFactory<T1> payloadFactory)
        {
            return _context.GetOrAddPayload(payloadFactory);
        }

        T1 PipeContext.AddOrUpdatePayload<T1>(PayloadFactory<T1> addFactory, UpdatePayloadFactory<T1> updateFactory)
        {
            return _context.AddOrUpdatePayload(addFactory, updateFactory);
        }

        T FactoryContext<T>.Object => _context.Object;

        public TProperty PropertyValue
        {
            set => _writeProperty.Set(_context.Object, value);
        }
    }
}