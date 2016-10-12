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
namespace GreenPipes.Internals.Mapping
{
    using System.ComponentModel;
    using Reflection;


    public class ValueObjectMapper<T> :
        IObjectMapper<T>
    {
        readonly ReadWriteProperty<T> _property;
        readonly TypeConverter _typeConverter;

        public ValueObjectMapper(ReadWriteProperty<T> property)
        {
            _property = property;
            _typeConverter = TypeDescriptor.GetConverter(property.Property.PropertyType);
        }

        public void ApplyTo(T obj, IObjectValueProvider valueProvider)
        {
            object value;
            if (valueProvider.TryGetValue(_property.Property.Name, out value))
            {
                if (value != null)
                {
                    var valueType = value.GetType();
                    if (!valueType.IsInstanceOfType(_property.Property.PropertyType))
                        if (_typeConverter.IsValid(value))
                            if (_typeConverter.CanConvertFrom(valueType))
                                value = _typeConverter.ConvertFrom(value);
                }

                if (value != null)
                    _property.Set(obj, value);
            }
        }
    }
}