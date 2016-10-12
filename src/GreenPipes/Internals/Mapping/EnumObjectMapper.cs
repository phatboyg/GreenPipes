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
    using System;
    using Reflection;


    public class EnumObjectMapper<T> :
        IObjectMapper<T>
    {
        readonly ReadWriteProperty<T> _property;

        public EnumObjectMapper(ReadWriteProperty<T> property)
        {
            _property = property;
        }

        public void ApplyTo(T obj, IObjectValueProvider valueProvider)
        {
            object value;
            if (!valueProvider.TryGetValue(_property.Property.Name, out value))
                return;
            if (value == null)
                return;

            if (value is T)
            {
                _property.Set(obj, value);
                return;
            }

            var s = value as string;
            if (s != null)
            {
                var enumValue = Enum.Parse(_property.Property.PropertyType, s);
                _property.Set(obj, enumValue);
                return;
            }

            var n = Enum.ToObject(_property.Property.PropertyType, value);
            _property.Set(obj, n);
        }
    }
}