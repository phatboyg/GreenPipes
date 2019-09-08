namespace GreenPipes
{
    using System;
    using System.Reflection;
    using System.Threading;
    using Payloads;


    public class ScopePipeContext
    {
        readonly PipeContext _context;
        IPayloadCache _payloadCache;

        public ScopePipeContext(PipeContext context)
        {
            _context = context;
        }

        public CancellationToken CancellationToken => _context.CancellationToken;

        IPayloadCache PayloadCache
        {
            get
            {
                if (_payloadCache != null)
                    return _payloadCache;

                while (Volatile.Read(ref _payloadCache) == null)
                    Interlocked.CompareExchange(ref _payloadCache, new ListPayloadCache(), null);

                return _payloadCache;
            }
        }

        public bool HasPayloadType(Type payloadType)
        {
            return payloadType.GetTypeInfo().IsInstanceOfType(this) || PayloadCache.HasPayloadType(payloadType) || _context.HasPayloadType(payloadType);
        }

        public bool TryGetPayload<T>(out T payload)
            where T : class
        {
            if (this is T context)
            {
                payload = context;
                return true;
            }

            return PayloadCache.TryGetPayload(out payload) || _context.TryGetPayload(out payload);
        }

        public T GetOrAddPayload<T>(PayloadFactory<T> payloadFactory)
            where T : class
        {
            if (this is T context)
                return context;

            if (PayloadCache.TryGetPayload<T>(out var payload))
                return payload;

            if (_context.TryGetPayload(out payload))
                return payload;

            return PayloadCache.GetOrAddPayload(payloadFactory);
        }

        public T AddOrUpdatePayload<T>(PayloadFactory<T> addFactory, UpdatePayloadFactory<T> updateFactory)
            where T : class
        {
            if (this is T context)
                return context;

            if (PayloadCache.TryGetPayload<T>(out var payload))
            {
                return PayloadCache.AddOrUpdatePayload(addFactory, updateFactory);
            }

            if (_context.TryGetPayload(out payload))
            {
                T Add() => updateFactory(payload);

                return PayloadCache.AddOrUpdatePayload(Add, updateFactory);
            }

            return PayloadCache.AddOrUpdatePayload(addFactory, updateFactory);
        }
    }
}