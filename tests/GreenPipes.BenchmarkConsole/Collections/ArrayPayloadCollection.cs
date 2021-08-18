namespace GreenPipes.BenchmarkConsole.Collections
{
    using System;
    using System.Linq;


    public class ArrayPayloadCollection :
        BasePayloadCollection
    {
        readonly IReadOnlyPayloadCollection _parent;
        readonly IPayloadValue[] _payloads;

        public ArrayPayloadCollection(IReadOnlyPayloadCollection parent, params IPayloadValue[] payloads)
            : base(parent)
        {
            _payloads = payloads;
            _parent = parent;
        }

        ArrayPayloadCollection(IReadOnlyPayloadCollection parent, IPayloadValue payload, IPayloadValue[] payloads)
            : base(parent)
        {
            _parent = parent;

            _payloads = new IPayloadValue[payloads.Length + 1];
            _payloads[0] = payload;
            Array.Copy(payloads, 0, _payloads, 1, payloads.Length);
        }

        public override bool HasPayloadType(Type payloadType)
        {
            if (_payloads.Any(x => x.Implements(payloadType)))
                return true;

            return base.HasPayloadType(payloadType);
        }

        public override bool TryGetPayload<TPayload>(out TPayload payload)
        {
            for (var i = 0; i < _payloads.Length; i++)
            {
                if (_payloads[i].TryGetValue(out payload))
                    return true;
            }

            return base.TryGetPayload(out payload);
        }

        public override IPayloadCollection Add(IPayloadValue payload)
        {
            return new ArrayPayloadCollection(_parent, payload, _payloads);
        }
    }
}
