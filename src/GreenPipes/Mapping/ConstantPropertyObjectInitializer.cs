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
    using System.Threading.Tasks;
    using Internals.Reflection;


    /// <summary>
    /// Initialize an object property to a constant value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    public class ConstantPropertyObjectInitializer<T, TProperty> :
        IObjectInitializer<T>
        where T : class
    {
        readonly TProperty _propertyValue;
        readonly IWriteProperty<T, TProperty> _writeProperty;

        public ConstantPropertyObjectInitializer(string propertyName, TProperty propertyValue)
        {
            _propertyValue = propertyValue;

            _writeProperty = WritePropertyCache<T>.GetProperty<TProperty>(propertyName);
        }

        public void Initialize(FactoryContext context, T value)
        {
            _writeProperty.Set(value, _propertyValue);
        }
    }


    /// <summary>
    /// Initialize an object property to a constant value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GuidPropertyObjectInitializer<T> :
        IObjectInitializer<T>
        where T : class
    {
        readonly IWriteProperty<T, Guid> _writeProperty;

        public GuidPropertyObjectInitializer(string propertyName)
        {
            _writeProperty = WritePropertyCache<T>.GetProperty<Guid>(propertyName);
        }

        public void Initialize(FactoryContext context, T value)
        {
            var propertyValue = Guid.NewGuid();

            _writeProperty.Set(value, propertyValue);
        }
    }


    public interface IPropertyValueProvider<in T, TProperty>
        where T : class
    {
        ValueTask<TProperty> Get(PropertyInitializerContext<T, TProperty> context);
    }


    public class ConstantPropertyValueProvider<T, TProperty> :
        IPropertyValueProvider<T, TProperty>
        where T : class
    {
        readonly TProperty _propertyValue;

        public ConstantPropertyValueProvider(TProperty propertyValue)
        {
            _propertyValue = propertyValue;
        }

        public ValueTask<TProperty> Get(PropertyInitializerContext<T, TProperty> context)
        {
            return new ValueTask<TProperty>(_propertyValue);
        }
    }
}