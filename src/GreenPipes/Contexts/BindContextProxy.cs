namespace GreenPipes.Contexts
{
    using System;
    using System.Reflection;
    using System.Threading;


    /// <summary>
    /// The BindContext
    /// </summary>
    /// <typeparam name="TLeft"></typeparam>
    /// <typeparam name="TRight"></typeparam>
    public class BindContextProxy<TLeft, TRight> :
        BindContext<TLeft, TRight>
        where TLeft : class, PipeContext
        where TRight : class
    {
        readonly TLeft _left;
        readonly TRight _right;

        public BindContextProxy(TLeft left, TRight source)
        {
            _left = left;
            _right = source;
        }

        TLeft BindContext<TLeft, TRight>.Left => _left;

        TRight BindContext<TLeft, TRight>.Right => _right;

        CancellationToken PipeContext.CancellationToken => _left.CancellationToken;

        bool PipeContext.HasPayloadType(Type payloadType)
        {
            return payloadType.GetTypeInfo().IsInstanceOfType(_right) || _left.HasPayloadType(payloadType);
        }

        bool PipeContext.TryGetPayload<T>(out T payload)
        {
            if (_right is T context)
            {
                payload = context;
                return true;
            }

            return _left.TryGetPayload(out payload);
        }

        T PipeContext.GetOrAddPayload<T>(PayloadFactory<T> payloadFactory)
        {
            if (_right is T context)
                return context;

            return _left.GetOrAddPayload(payloadFactory);
        }

        T PipeContext.AddOrUpdatePayload<T>(PayloadFactory<T> addFactory, UpdatePayloadFactory<T> updateFactory)
        {
            if (_right is T context)
                return context;

            return _left.AddOrUpdatePayload(addFactory, updateFactory);
        }
    }
}