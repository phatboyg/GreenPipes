namespace GreenPipes.Internals.Extensions
{
    using Reflection;


    public interface ITypeCache<T>
    {
        string ShortName { get; }
        IReadOnlyPropertyCache<T> ReadOnlyPropertyCache { get; }
        IReadWritePropertyCache<T> ReadWritePropertyCache { get; }

        T InitializeFromObject(object values);
    }
}
