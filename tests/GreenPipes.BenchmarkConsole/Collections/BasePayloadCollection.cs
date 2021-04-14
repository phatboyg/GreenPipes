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
namespace GreenPipes.BenchmarkConsole.Collections
{
    using System;


    public abstract class BasePayloadCollection :
        IPayloadCollection
    {
        readonly IReadOnlyPayloadCollection _parent;

        protected BasePayloadCollection(IReadOnlyPayloadCollection parent)
        {
            _parent = parent;
        }

        protected IReadOnlyPayloadCollection Parent => _parent;

        public virtual bool HasPayloadType(Type payloadType)
        {
            return _parent?.HasPayloadType(payloadType) ?? false;
        }

        public virtual bool TryGetPayload<TPayload>(out TPayload payload)
            where TPayload : class
        {
            if (_parent != null)
                return _parent.TryGetPayload(out payload);

            payload = null;
            return false;
        }

        public abstract IPayloadCollection Add(IPayloadValue payload);
    }
}