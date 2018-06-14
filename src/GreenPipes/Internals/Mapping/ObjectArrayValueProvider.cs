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
    using System;
    using System.Collections.Generic;


    public class ObjectArrayValueProvider :
        IArrayValueProvider
    {
        readonly Array _values;

        public ObjectArrayValueProvider(Array values)
        {
            _values = values;
        }

        public bool TryGetValue(int index, out object value)
        {
            if (index < 0 || index >= _values.Length)
            {
                value = null;
                return false;
            }

            value = _values.GetValue(index);
            if (value is IDictionary<string, object>)
                value = new DictionaryObjectValueProvider((IDictionary<string, object>)value);
            else if (value is Array)
                value = new ObjectArrayValueProvider((Array)value);

            return true;
        }

        public bool TryGetValue<T>(int index, out T value)
        {
            object obj;
            if (TryGetValue(index, out obj))
                if (obj is T)
                {
                    value = (T)obj;
                    return true;
                }

            value = default(T);
            return false;
        }
    }
}