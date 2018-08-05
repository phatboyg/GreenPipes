namespace GreenPipes.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Internals.Extensions;


    public class PropertySpecification<TObject, TProperty> :
        IPropertySpecification<TObject>
        where TObject : class
    {
        readonly IList<IInitializerSpecification<TObject>> _initializerSpecifications;
        readonly IList<IPropertyInitializerSpecification<TObject, TProperty>> _propertyiInitializerSpecifications;

        public PropertySpecification()
        {
            _initializerSpecifications = new List<IInitializerSpecification<TObject>>();
            _propertyiInitializerSpecifications = new List<IPropertyInitializerSpecification<TObject, TProperty>>();
        }

        public bool IsDefined => _initializerSpecifications.Count > 0 || _propertyiInitializerSpecifications.Count > 0;

        public void Add(IInitializerSpecification<TObject> specification)
        {
            _initializerSpecifications.Add(specification);
        }

        public void Add<T>(IPropertyInitializerSpecification<TObject, T> specification)
        {
            _propertyiInitializerSpecifications.Add(specification as IPropertyInitializerSpecification<TObject, TProperty> ??
                throw new ArgumentException($"Invalid specification type: {TypeCache<T>.ShortName}", nameof(specification)));
        }

        public IEnumerable<ValidationResult> Validate()
        {
            return _initializerSpecifications.SelectMany(x => x.Validate())
                .Concat(_propertyiInitializerSpecifications.SelectMany(x => x.Validate()));
        }
    }
}