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
namespace GreenPipes.Internals.Reflection
{
    using System;
    using System.Collections.Generic;
    using Extensions;


    public class ReadPropertyCache<T> :
        IReadPropertyCache<T>
    {
        readonly Type _implementationType;
        readonly IDictionary<string, IReadProperty<T>> _properties;

        ReadPropertyCache()
        {
            _implementationType = TypeCache<T>.ImplementationType;

            _properties = new Dictionary<string, IReadProperty<T>>(StringComparer.OrdinalIgnoreCase);
        }

        IReadProperty<T, TProperty> IReadPropertyCache<T>.GetProperty<TProperty>(string name)
        {
            lock (_properties)
            {
                if (_properties.TryGetValue(name, out var property))
                    return property as IReadProperty<T, TProperty>;

                var writeProperty = new ReadProperty<T, TProperty>(_implementationType, name);

                _properties[name] = writeProperty;

                return writeProperty;
            }
        }

        public static IReadProperty<T, TProperty> GetProperty<TProperty>(string name)
        {
            return Cached.PropertyCache.GetProperty<TProperty>(name);
        }


        static class Cached
        {
            internal static readonly IReadPropertyCache<T> PropertyCache = new ReadPropertyCache<T>();
        }
    }
}