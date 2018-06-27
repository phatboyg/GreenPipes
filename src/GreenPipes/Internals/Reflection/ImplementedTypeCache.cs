// Copyright 2012-2018 Chris Patterson
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
namespace GreenPipes.Internals.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;


    public class ImplementedTypeCache<T> :
        IImplementedTypeCache
        where T : class
    {
        readonly CachedType[] _implementedTypes;

        public ImplementedTypeCache()
        {
            _implementedTypes = GetImplementedTypes()
                .Where(x => x.Type != typeof(T))
                .Select(x => Activator.CreateInstance(typeof(TypeAdapter<>).MakeGenericType(typeof(T), x.Type), (object)x.Direct))
                .Cast<CachedType>()
                .ToArray();
        }

        void IImplementedTypeCache.EnumerateImplementedTypes(IImplementedType implementedType, bool includeActualType)
        {
            for (var i = 0; i < _implementedTypes.Length; i++)
            {
                if (_implementedTypes[i].MessageType == typeof(T) && !includeActualType)
                    continue;

                _implementedTypes[i].ImplementsType(implementedType);
            }
        }

        /// <summary>
        /// Enumerate the implemented types
        /// </summary>
        /// <param name="implementedType">The interface reference to invoke for each type</param>
        /// <param name="includeActualType">Include the actual message type first, before any implemented types</param>
        public static void EnumerateImplementedTypes(IImplementedType implementedType, bool includeActualType = false)
        {
            Cached.Instance.Value.EnumerateImplementedTypes(implementedType, includeActualType);
        }

        static IEnumerable<ImplementedType> GetImplementedTypes()
        {
            yield return new ImplementedType(typeof(T), true);

            var implementedInterfaces = GetImplementedInterfaces(typeof(T)).ToArray();

            foreach (var baseInterface in implementedInterfaces.Except(implementedInterfaces.SelectMany(x => x.GetInterfaces())))
                yield return new ImplementedType(baseInterface, true);

            foreach (var baseInterface in implementedInterfaces.SelectMany(x => x.GetInterfaces()).Distinct())
                yield return new ImplementedType(baseInterface, false);

            var baseType = typeof(T).GetTypeInfo().BaseType;
            while (baseType != null)
            {
                yield return new ImplementedType(baseType, typeof(T).GetTypeInfo().BaseType == baseType);

                foreach (var baseInterface in GetImplementedInterfaces(baseType))
                    yield return new ImplementedType(baseInterface, false);

                baseType = baseType.GetTypeInfo().BaseType;
            }
        }

        static IEnumerable<Type> GetImplementedInterfaces(Type baseType)
        {
            var baseTypeInfo = baseType.GetTypeInfo();

            IEnumerable<Type> baseInterfaces = baseTypeInfo
                .GetInterfaces()
                .ToArray();

            if (baseTypeInfo.BaseType != null && baseTypeInfo.BaseType != typeof(object))
            {
                baseInterfaces = baseInterfaces
                    .Except(baseTypeInfo.BaseType.GetInterfaces())
                    .Except(baseInterfaces.SelectMany(x => x.GetInterfaces()))
                    .ToArray();
            }

            return baseInterfaces;
        }


        struct ImplementedType
        {
            /// <summary>
            /// The implemented type
            /// </summary>
            public readonly Type Type;

            /// <summary>
            /// True if the interface is directly implemented by the type
            /// </summary>
            public readonly bool Direct;

            public ImplementedType(Type type, bool direct)
            {
                Type = type;
                Direct = direct;
            }
        }


        interface CachedType
        {
            Type MessageType { get; }
            bool Direct { get; }
            void ImplementsType(IImplementedType implementedType);
        }


        static class Cached
        {
            internal static readonly Lazy<IImplementedTypeCache> Instance = new Lazy<IImplementedTypeCache>(
                () => new ImplementedTypeCache<T>(), LazyThreadSafetyMode.PublicationOnly);
        }


        class TypeAdapter<TAdapter> :
            CachedType
            where TAdapter : class
        {
            public TypeAdapter(bool direct)
            {
                Direct = direct;
            }

            public bool Direct { get; }
            Type CachedType.MessageType => typeof(TAdapter);

            void CachedType.ImplementsType(IImplementedType implementedType)
            {
                implementedType.ImplementsType<TAdapter>(Direct);
            }
        }
    }
}