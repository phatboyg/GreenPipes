namespace GreenPipes.Internals.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Reflection;


    public static class InterfaceExtensions
    {
        static readonly InterfaceReflectionCache _cache;

        static InterfaceExtensions()
        {
            _cache = new InterfaceReflectionCache();
        }

        public static bool HasInterface<T>(this Type type)
        {
            return HasInterface(type, typeof(T));
        }

        public static bool HasInterface(this Type type, Type interfaceType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));

            var interfaceTypeInfo = interfaceType.GetTypeInfo();
            if (!interfaceTypeInfo.IsInterface)
                throw new ArgumentException("The interface type must be an interface: " + interfaceType.Name);

            if (interfaceTypeInfo.IsGenericTypeDefinition)
                return _cache.GetGenericInterface(type, interfaceTypeInfo) != null;

            return interfaceTypeInfo.IsAssignableFrom(type.GetTypeInfo());
        }

        public static Type GetInterface<T>(this Type type)
        {
            return GetInterface(type, typeof(T));
        }

        public static Type GetInterface(this Type type, Type interfaceType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));

            var interfaceTypeInfo = interfaceType.GetTypeInfo();
            if (!interfaceTypeInfo.IsInterface)
                throw new ArgumentException("The interface type must be an interface: " + interfaceType.Name);

            return _cache.Get(type, interfaceTypeInfo);
        }

        public static bool ClosesType(this Type type, Type openType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (openType == null)
                throw new ArgumentNullException(nameof(openType));

            if (!openType.IsOpenGeneric())
                throw new ArgumentException("The interface type must be an open generic interface: " + openType.Name);

            if (openType.GetTypeInfo().IsInterface)
            {
                if (!openType.IsOpenGeneric())
                    throw new ArgumentException("The interface type must be an open generic interface: " + openType.Name);

                var interfaceType = type.GetInterface(openType);
                if (interfaceType == null)
                    return false;

                var typeInfo = interfaceType.GetTypeInfo();
                return !typeInfo.IsGenericTypeDefinition && !typeInfo.ContainsGenericParameters;
            }

            var baseType = type;
            while (baseType != null && baseType != typeof(object))
            {
                var baseTypeInfo = baseType.GetTypeInfo();
                if (baseTypeInfo.IsGenericType && baseTypeInfo.GetGenericTypeDefinition() == openType)
                    return !baseTypeInfo.IsGenericTypeDefinition && !baseTypeInfo.ContainsGenericParameters;

                if (!baseTypeInfo.IsGenericType && baseType == openType)
                    return true;

                baseType = baseTypeInfo.BaseType;
            }

            return false;
        }

        public static IEnumerable<Type> GetClosingArguments(this Type type, Type openType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (openType == null)
                throw new ArgumentNullException(nameof(openType));

            if (!openType.IsOpenGeneric())
                throw new ArgumentException("The interface type must be an open generic interface: " + openType.Name);

            if (openType.GetTypeInfo().IsInterface)
            {
                var interfaceType = type.GetInterface(openType);
                if (interfaceType == null)
                    throw new ArgumentException("The interface type is not implemented by: " + type.Name);

                return interfaceType.GetTypeInfo().GetGenericArguments().Where(x => !x.IsGenericParameter);
            }

            var baseType = type;
            while (baseType != null && baseType != typeof(object))
            {
                var baseTypeInfo = baseType.GetTypeInfo();
                if (baseTypeInfo.IsGenericType && baseType.GetGenericTypeDefinition() == openType)
                    return baseTypeInfo.GetGenericArguments().Where(x => !x.IsGenericParameter);

                if (!baseTypeInfo.IsGenericType && baseType == openType)
                    return baseTypeInfo.GetGenericArguments().Where(x => !x.IsGenericParameter);

                baseType = baseTypeInfo.BaseType;
            }

            throw new ArgumentException("Could not find open type in type: " + type.Name);
        }
    }
}
