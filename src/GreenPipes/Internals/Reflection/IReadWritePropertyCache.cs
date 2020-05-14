namespace GreenPipes.Internals.Reflection
{
    using System.Collections.Generic;


    public interface IReadWritePropertyCache<T> :
        IEnumerable<ReadWriteProperty<T>>
    {
        ReadWriteProperty<T> this[string name] { get; }
        bool TryGetValue(string key, out ReadWriteProperty<T> value);
        bool TryGetProperty(string propertyName, out ReadWriteProperty<T> property);
    }
}
