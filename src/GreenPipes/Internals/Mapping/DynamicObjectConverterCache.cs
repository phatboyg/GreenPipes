namespace GreenPipes.Internals.Mapping
{
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;
    using Reflection;


    /// <summary>
    /// Caches dictionary to object converters for the types requested, including the implementation
    /// builder for interfaces that are dynamically proxied
    /// </summary>
    public class DynamicObjectConverterCache :
        IObjectConverterCache
    {
        readonly ConcurrentDictionary<Type, IObjectConverter> _cache;
        readonly IImplementationBuilder _implementationBuilder;

        public DynamicObjectConverterCache(IImplementationBuilder implementationBuilder)
        {
            _implementationBuilder = implementationBuilder;
            _cache = new ConcurrentDictionary<Type, IObjectConverter>();
        }

        public IObjectConverter GetConverter(Type type)
        {
            return _cache.GetOrAdd(type, CreateMissingConverter);
        }

        IObjectConverter CreateMissingConverter(Type type)
        {
            var implementationType = type.GetTypeInfo().IsInterface ? _implementationBuilder.GetImplementationType(type) : type;
            var converterType = typeof(DynamicObjectConverter<,>).MakeGenericType(type, implementationType);

            return (IObjectConverter)Activator.CreateInstance(converterType, this);
        }
    }
}
