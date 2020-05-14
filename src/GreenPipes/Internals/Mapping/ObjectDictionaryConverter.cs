namespace GreenPipes.Internals.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Extensions;
    using Reflection;


    public class ObjectDictionaryConverter<T> :
        IDictionaryConverter
        where T : class
    {
        readonly DictionaryConverterCache _cache;
        readonly IDictionaryMapper<T>[] _mappers;

        public ObjectDictionaryConverter(DictionaryConverterCache cache)
        {
            _cache = cache;
            _mappers = TypeCache<T>.ReadOnlyPropertyCache
                .Select(property => GetDictionaryMapper(property, property.Property.PropertyType))
                .ToArray();
        }

        public IDictionary<string, object> GetDictionary(object obj)
        {
            var dictionary = new Dictionary<string, object>();
            if (obj == null)
                return dictionary;

            var value = (T)obj;
            for (var i = 0; i < _mappers.Length; i++)
                _mappers[i].WritePropertyToDictionary(dictionary, value);

            return dictionary;
        }

        IDictionaryMapper<T> GetDictionaryMapper(ReadOnlyProperty<T> property, Type valueType)
        {
            Type underlyingType;
            if (valueType.IsNullable(out underlyingType))
            {
                var converterType = typeof(NullableValueDictionaryMapper<,>).MakeGenericType(typeof(T),
                    underlyingType);

                return (IDictionaryMapper<T>)Activator.CreateInstance(converterType, property);
            }

            if (valueType.GetTypeInfo().IsEnum)
                return new EnumDictionaryMapper<T>(property);

            if (valueType.IsArray)
            {
                var elementType = valueType.GetElementType();
                if (ValueObject.IsValueObjectType(elementType))
                    return new ValueDictionaryMapper<T>(property);

                var elementConverter = _cache.GetConverter(elementType);

                var converterType = typeof(ObjectArrayDictionaryMapper<,>).MakeGenericType(typeof(T), elementType);
                return (IDictionaryMapper<T>)Activator.CreateInstance(converterType, property, elementConverter);
            }

            if (ValueObject.IsValueObjectType(valueType))
                return new ValueDictionaryMapper<T>(property);

            if (valueType.HasInterface(typeof(IEnumerable<>)))
            {
                var elementType = valueType.GetClosingArguments(typeof(IEnumerable<>)).Single();

                if (valueType.ClosesType(typeof(IDictionary<,>)))
                {
                    Type[] arguments = valueType.GetClosingArguments(typeof(IDictionary<,>)).ToArray();
                    var keyType = arguments[0];
                    if (ValueObject.IsValueObjectType(keyType))
                    {
                        elementType = arguments[1];
                        if (ValueObject.IsValueObjectType(elementType))
                        {
                            var converterType = typeof(ValueValueDictionaryDictionaryMapper<,,>).MakeGenericType(typeof(T), keyType, elementType);
                            return (IDictionaryMapper<T>)Activator.CreateInstance(converterType, property);
                        }
                        else
                        {
                            var converterType = typeof(ValueObjectDictionaryDictionaryMapper<,,>).MakeGenericType(typeof(T), keyType, elementType);
                            var elementConverter = _cache.GetConverter(elementType);
                            return (IDictionaryMapper<T>)Activator.CreateInstance(converterType, property, elementConverter);
                        }
                    }

                    throw new InvalidOperationException("Unable to map a reference type key dictionary");
                }

                if (valueType.ClosesType(typeof(IList<>)) || valueType.ClosesType(typeof(IEnumerable<>)))
                {
                    var converterType = typeof(ObjectListDictionaryMapper<,>).MakeGenericType(typeof(T), elementType);
                    var elementConverter = _cache.GetConverter(elementType);

                    return (IDictionaryMapper<T>)Activator.CreateInstance(converterType, property, elementConverter);
                }
            }

            return new ObjectDictionaryMapper<T>(property, _cache.GetConverter(valueType));
        }
    }
}
