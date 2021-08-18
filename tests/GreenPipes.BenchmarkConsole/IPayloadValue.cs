namespace GreenPipes.BenchmarkConsole
{
    using System;


    /// <summary>
    /// A property is a value stored in the context, which can be accessed by name or
    /// by type. This is the actual property storage element
    /// </summary>
    public interface IPayloadValue
    {
        /// <summary>
        /// The property value type
        /// </summary>
        Type ValueType { get; }

        /// <summary>
        /// Checks if the payload value implements the <paramref name="type"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool Implements(Type type);

        /// <summary>
        /// Returns the value if it can be assigned to the specified type
        /// </summary>
        /// <typeparam name="T">The requested type</typeparam>
        /// <param name="value">The output value</param>
        /// <returns></returns>
        bool TryGetValue<T>(out T value)
            where T : class;
    }


    /// <summary>
    /// A property value with the generic type applied
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    public interface IPayloadValue<out TPayload> :
        IPayloadValue
        where TPayload : class
    {
        /// <summary>
        /// The value of the property, already assigned to T
        /// </summary>
        TPayload Value { get; }
    }
}
