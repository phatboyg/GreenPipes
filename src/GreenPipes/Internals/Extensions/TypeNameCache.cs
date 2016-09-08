// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using System.Threading;
    using Introspection;
    using Newtonsoft.Json.Linq;
    using Reflection;


    public static class TypeNameCache
    {
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
            public string ShortName => TypeNameCache<T>.ShortName;
        }
    }


    public interface ITypeNameCache<out T>
    {
        string ShortName { get; }

        T InitializeFromObject(object values);
    }


    public class TypeNameCache<T> :
        ITypeNameCache<T>
    {
        readonly string _shortName;

        TypeNameCache()
        {
            _shortName = typeof(T).GetTypeName();
        }

        public static string ShortName => Cached.Metadata.Value.ShortName;

        T ITypeNameCache<T>.InitializeFromObject(object values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var objValues = JObject.FromObject(values, SerializerCache.Serializer);

            return objValues.ToObject<T>(SerializerCache.Serializer);
        }

        string ITypeNameCache<T>.ShortName => _shortName;

        public static T InitializeFromObject(object values)
        {
            return Cached.Metadata.Value.InitializeFromObject(values);
        }


        static class Cached
        {
            internal static readonly Lazy<ITypeNameCache<T>> Metadata = new Lazy<ITypeNameCache<T>>(
                () => new TypeNameCache<T>(), LazyThreadSafetyMode.PublicationOnly);
        }
    }
}