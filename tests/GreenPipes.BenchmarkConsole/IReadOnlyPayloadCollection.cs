namespace GreenPipes.BenchmarkConsole
{
    using System;


    /// <summary>
    /// Supports the reading of the property cache
    /// </summary>
    public interface IReadOnlyPayloadCollection
    {
        /// <summary>
        /// Checks if the property exists in the cache
        /// </summary>
        /// <param name="payloadType">The property type</param>
        /// <returns>True if the property exists in the cache, otherwise false</returns>
        bool HasPayloadType(Type payloadType);

        /// <summary>
        /// Returns the value of the property if it exists in the cache
        /// </summary>
        /// <typeparam name="TPayload">The property type</typeparam>
        /// <param name="payload">The property value</param>
        /// <returns>True if the value was returned, otherwise false</returns>
        bool TryGetPayload<TPayload>(out TPayload payload)
            where TPayload : class;
    }
}
