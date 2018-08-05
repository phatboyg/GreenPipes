namespace GreenPipes.Mapping
{
    using System;
    using System.Collections.Generic;


    public class ObjectInitializerSpecification<T>
        where T : class
    {
        readonly IDictionary<string, IPropertySpecification<T>> _propertySpecifications;

        public ObjectInitializerSpecification()
        {
            _propertySpecifications = new Dictionary<string, IPropertySpecification<T>>(StringComparer.OrdinalIgnoreCase);
        }
    }
}