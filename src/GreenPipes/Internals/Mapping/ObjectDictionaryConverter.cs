// Copyright 2012-2016 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace GreenPipes.Internals.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

            if (valueType.IsEnum)
                return new EnumDictionaryMapper<T>(property);

            if (valueType.IsArray)
            {
                var elementType = valueType.GetElementType();
                if (elementType.IsValueType || (elementType == typeof(string)))
                    return new ValueDictionaryMapper<T>(property);

                var elementConverter = _cache.GetConverter(elementType);

                var converterType = typeof(ObjectArrayDictionaryMapper<,>).MakeGenericType(typeof(T), elementType);
                return (IDictionaryMapper<T>)Activator.CreateInstance(converterType, property, elementConverter);
            }

            if (valueType.IsValueType || (valueType == typeof(string)) || typeof(Exception).IsAssignableFrom(valueType))
                return new ValueDictionaryMapper<T>(property);

            if (valueType.HasInterface(typeof(IEnumerable<>)))
            {
                var elementType = valueType.GetClosingArguments(typeof(IEnumerable<>)).Single();

                if (valueType.ClosesType(typeof(IDictionary<,>)))
                {
                    Type[] arguments = valueType.GetClosingArguments(typeof(IDictionary<,>)).ToArray();
                    var keyType = arguments[0];
                    if (keyType.IsValueType || (keyType == typeof(string)))
                    {
                        elementType = arguments[1];
                        if (elementType.IsValueType || (elementType == typeof(string)))
                        {
                            var converterType =
                                typeof(ValueValueDictionaryDictionaryMapper<,,>).MakeGenericType(typeof(T),
                                    keyType, elementType);
                            return (IDictionaryMapper<T>)Activator.CreateInstance(converterType, property);
                        }
                        else
                        {
                            var converterType =
                                typeof(ValueObjectDictionaryDictionaryMapper<,,>).MakeGenericType(typeof(T),
                                    keyType, elementType);
                            var elementConverter = _cache.GetConverter(elementType);
                            return
                                (IDictionaryMapper<T>)Activator.CreateInstance(converterType, property, elementConverter);
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