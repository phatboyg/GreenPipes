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
namespace GreenPipes.Payloads.Collections
{
    using System;
    using System.Reflection;

    public class SinglePayloadCollection :
        BasePayloadCollection
    {
        readonly IReadOnlyPayloadCollection _parent;
        readonly IPayloadValue _payload;

        public SinglePayloadCollection(IPayloadValue payload, IReadOnlyPayloadCollection parent = null)
            : base(parent)
        {
            _payload = payload;
            _parent = parent;
        }

        public override bool HasPayloadType(Type payloadType)
        {
            if (payloadType.GetTypeInfo().IsAssignableFrom(_payload.ValueType))
                return true;

            return base.HasPayloadType(payloadType);
        }

        public override bool TryGetPayload<TPayload>(out TPayload payload)
        {
            TPayload payloadValue;
            if (_payload.TryGetValue(out payloadValue))
            {
                payload = payloadValue;
                return true;
            }

            return base.TryGetPayload(out payload);
        }

        public override IPayloadCollection Add(IPayloadValue payload)
        {
            return new ArrayPayloadCollection(_parent, payload, _payload);
        }
    }
}