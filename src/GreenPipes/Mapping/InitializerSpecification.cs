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
namespace GreenPipes.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Internals.Extensions;


    public abstract class InitializerSpecification<T, TProperty> :
        IInitializerSpecification<T>
        where T : class
    {
        protected InitializerSpecification(Expression<Func<T, TProperty>> propertyExpression)
        {
            Property = propertyExpression.GetPropertyInfo();
        }

        protected InitializerSpecification(PropertyInfo property)
        {
            Property = property;
        }

        protected PropertyInfo Property { get; }

        IEnumerable<ValidationResult> ISpecification.Validate()
        {
            if (Property == null)
                yield return this.Failure("Property not specified", $"{TypeCache<T>.ShortName}.{Property?.Name}");

            if (Property != null)
            {
                var propertyInfo = typeof(T).GetProperty(Property.Name);
                if (propertyInfo == null)
                    yield return this.Failure("Property not found", $"{TypeCache<T>.ShortName}.{Property.Name}");
                else
                {
                    if (propertyInfo.PropertyType != Property.PropertyType)
                    {
                        yield return this.Failure("Property type mismatch",
                            $"{TypeCache<T>.ShortName}.{Property?.Name} ({TypeCache.GetShortName(Property.PropertyType)})");
                    }
                }
            }

            foreach (var result in Validate())
                yield return result;
        }

        public virtual IEnumerable<Type> GetReferencedTypes()
        {
            yield break;
        }

        public abstract void Apply(IInitializerBuilder<T> builder);

        protected abstract IEnumerable<ValidationResult> Validate();
    }
}