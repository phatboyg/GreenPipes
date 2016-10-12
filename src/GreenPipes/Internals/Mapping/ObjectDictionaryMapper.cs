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
    using System.Collections.Generic;
    using Reflection;


    public class ObjectDictionaryMapper<T> :
        IDictionaryMapper<T>
    {
        readonly IDictionaryConverter _converter;
        readonly ReadOnlyProperty<T> _property;

        public ObjectDictionaryMapper(ReadOnlyProperty<T> property, IDictionaryConverter converter)
        {
            _property = property;
            _converter = converter;
        }

        public void WritePropertyToDictionary(IDictionary<string, object> dictionary, T obj)
        {
            IDictionary<string, object> value = _converter.GetDictionary(_property.Get(obj));

            dictionary.Add(_property.Property.Name, value);
        }
    }
}