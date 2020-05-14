namespace GreenPipes.Internals.Mapping
{
    using System;
    using System.Collections.Concurrent;


    /// <summary>
    /// Caches the type converter instances
    /// </summary>
    public class DictionaryConverterCache :
        IDictionaryConverterCache
    {
        readonly ConcurrentDictionary<Type, IDictionaryConverter> _cache;

        public DictionaryConverterCache()
        {
            _cache = new ConcurrentDictionary<Type, IDictionaryConverter>();
        }

        public IDictionaryConverter GetConverter(Type type)
        {
            return _cache.GetOrAdd(type, CreateMissingConverter);
        }

        IDictionaryConverter CreateMissingConverter(Type key)
        {
            var type = typeof(ObjectDictionaryConverter<>).MakeGenericType(key);

            return (IDictionaryConverter)Activator.CreateInstance(type, this);
        }
    }
}
