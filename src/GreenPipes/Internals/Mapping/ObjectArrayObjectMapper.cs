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
namespace GreenPipes.Internals.Mapping
{
    using System.Collections.Generic;
    using Reflection;


    public class ObjectArrayObjectMapper<T, TElement> :
        IObjectMapper<T>
    {
        readonly IObjectConverter _converter;
        readonly ReadWriteProperty<T> _property;

        public ObjectArrayObjectMapper(ReadWriteProperty<T> property,
            IObjectConverter converter)
        {
            _property = property;
            _converter = converter;
        }

        public void ApplyTo(T obj, IObjectValueProvider valueProvider)
        {
            IArrayValueProvider values;
            if (!valueProvider.TryGetValue(_property.Property.Name, out values))
                return;

            var elements = new List<TElement>();

            for (var i = 0;; i++)
            {
                IObjectValueProvider elementValueProvider;
                if (!values.TryGetValue(i, out elementValueProvider))
                    break;

                var element = (TElement)_converter.GetObject(elementValueProvider);
                elements.Add(element);
            }

            _property.Set(obj, elements.ToArray());
        }
    }
}