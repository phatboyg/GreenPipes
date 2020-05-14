namespace GreenPipes.Internals.Mapping
{
    using System;
    using System.Reflection;


    public static class ValueObject
    {
        public static bool IsValueObjectType(Type valueType)
        {
            if (valueType.GetTypeInfo().IsValueType
                || valueType == typeof(string)
                || valueType == typeof(Uri)
                || valueType == typeof(Version)
                || typeof(Exception).IsAssignableFrom(valueType))
                return true;

            return false;
        }
    }
}
