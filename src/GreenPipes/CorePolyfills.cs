namespace System
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate)]
    internal class SerializableAttribute : Attribute
    {
    }
}