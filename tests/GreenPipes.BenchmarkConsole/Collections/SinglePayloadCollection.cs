namespace GreenPipes.BenchmarkConsole.Collections
{
    using System;


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
            if (_payload.Implements(payloadType))
                return true;

            return base.HasPayloadType(payloadType);
        }

        public override bool TryGetPayload<TPayload>(out TPayload payload)
        {
            if (_payload.TryGetValue(out TPayload payloadValue))
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
