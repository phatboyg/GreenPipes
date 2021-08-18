namespace GreenPipes.BenchmarkConsole
{
    using System;
    using Internals.Extensions;


    /// <summary>
    /// Stores a single scope data value
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    public class PayloadValue<TPayload> :
        IPayloadValue<TPayload>
        where TPayload : class
    {
        readonly TPayload _value;

        public PayloadValue(TPayload value)
        {
            if (value == default(TPayload))
                throw new PayloadNotFoundException($"The payload was not found: {TypeCache<TPayload>.ShortName}");

            _value = value;
        }

        Type IPayloadValue.ValueType => typeof(TPayload);
        TPayload IPayloadValue<TPayload>.Value => _value;

        bool IPayloadValue.Implements(Type type)
        {
            return type.IsInstanceOfType(_value);
        }

        bool IPayloadValue.TryGetValue<T>(out T value)
        {
            value = _value as T;

            return value != null;
        }
    }
}
