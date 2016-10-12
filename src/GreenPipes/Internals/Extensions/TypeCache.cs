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
namespace GreenPipes.Internals.Extensions
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using Mapping;
    using Reflection;


    public static class TypeCache
    {
        internal static readonly IDictionaryConverterCache DictionaryConverterCache = new DictionaryConverterCache();
        internal static readonly IObjectConverterCache ObjectConverterCache = new DynamicObjectConverterCache(Cached.Builder);
        public static IImplementationBuilder ImplementationBuilder => Cached.Builder;

        static CachedType GetOrAdd(Type type)
        {
            return Cached.Instance.GetOrAdd(type, _ =>
                (CachedType)Activator.CreateInstance(typeof(CachedType<>).MakeGenericType(type)));
        }

        public static string GetShortName(Type type)
        {
            return GetOrAdd(type).ShortName;
        }

        public static Type GetImplementationType(Type type)
        {
            return Cached.Builder.GetImplementationType(type);
        }


        static class Cached
        {
            internal static readonly IImplementationBuilder Builder = new DynamicImplementationBuilder();
            internal static readonly ConcurrentDictionary<Type, CachedType> Instance = new ConcurrentDictionary<Type, CachedType>();
        }


        interface CachedType
        {
            string ShortName { get; }
        }


        class CachedType<T> :
            CachedType
        {
            public string ShortName => TypeCache<T>.ShortName;
        }
    }


    public class TypeCache<T> :
        ITypeCache<T>
    {
        readonly Lazy<IObjectConverter> _converter;
        readonly Lazy<IDictionaryConverter> _mapper;
        readonly Lazy<ReadOnlyPropertyCache<T>> _readPropertyCache;
        readonly string _shortName;
        readonly Lazy<ReadWritePropertyCache<T>> _writePropertyCache;

        TypeCache()
        {
            _shortName = typeof(T).GetTypeName();
            _readPropertyCache = new Lazy<ReadOnlyPropertyCache<T>>(() => new ReadOnlyPropertyCache<T>());
            _writePropertyCache = new Lazy<ReadWritePropertyCache<T>>(() => new ReadWritePropertyCache<T>());

            _mapper = new Lazy<IDictionaryConverter>(() => TypeCache.DictionaryConverterCache.GetConverter(typeof(T)));
            _converter = new Lazy<IObjectConverter>(() => TypeCache.ObjectConverterCache.GetConverter(typeof(T)));
        }

        public static IReadOnlyPropertyCache<T> ReadOnlyPropertyCache => Cached.Metadata.Value.ReadOnlyPropertyCache;
        public static IReadWritePropertyCache<T> ReadWritePropertyCache => Cached.Metadata.Value.ReadWritePropertyCache;

        public static string ShortName => Cached.Metadata.Value.ShortName;

        IReadOnlyPropertyCache<T> ITypeCache<T>.ReadOnlyPropertyCache => _readPropertyCache.Value;
        IReadWritePropertyCache<T> ITypeCache<T>.ReadWritePropertyCache => _writePropertyCache.Value;

        T ITypeCache<T>.InitializeFromObject(object values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            IDictionary<string, object> dictionary = TypeCache.DictionaryConverterCache.GetConverter(values.GetType()).GetDictionary(values);

            return (T)_converter.Value.GetObject(new DictionaryObjectValueProvider(dictionary));
        }

        string ITypeCache<T>.ShortName => _shortName;

        public static T InitializeFromObject(object values)
        {
            return Cached.Metadata.Value.InitializeFromObject(values);
        }


        static class Cached
        {
            internal static readonly Lazy<ITypeCache<T>> Metadata = new Lazy<ITypeCache<T>>(() => new TypeCache<T>(),
                LazyThreadSafetyMode.PublicationOnly);
        }
    }
}