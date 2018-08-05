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
    using System.Linq;


    public class InitializerBuilder<TObject> :
        IInitializerBuilder<TObject>
        where TObject : class
    {
        readonly IObjectFactory<TObject> _factory;
        readonly IList<IFactoryInitializer<TObject>> _factoryInitializers;
        readonly IDictionary<string, IPropertyInitializerBuilder<TObject>> _properties;

        public InitializerBuilder(IObjectFactory<TObject> factory)
        {
            _factory = factory;

            _factoryInitializers = new List<IFactoryInitializer<TObject>>();
            _properties = new Dictionary<string, IPropertyInitializerBuilder<TObject>>(StringComparer.OrdinalIgnoreCase);
        }

        public void Add(IFactoryInitializer<TObject> initializer)
        {
            _factoryInitializers.Add(initializer);
        }

        public IPropertyInitializerBuilder<TObject, T> Property<T>(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var property))
                return property as IPropertyInitializerBuilder<TObject, T>;

            var builder = new PropertyInitializerBuilder<TObject, T>(propertyName);

            _properties[propertyName] = builder;

            return builder;
        }

        public IObjectInitializer<TObject> Build()
        {
            var initializers = _properties.Select(x => x.Value.Build()).ToArray();

            return new ObjectInitializer<TObject>(_factory, _factoryInitializers.ToArray(), initializers);
        }
    }
}