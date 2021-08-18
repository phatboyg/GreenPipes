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
