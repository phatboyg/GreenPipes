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
    using Builders;
    using Configurators;


    public class PropertyInitializerBuilder<T, TProperty> :
        IPropertyInitializerBuilder<T, TProperty>,
        IPropertyInitializerBuilder<T>
        where T : class
    {
        readonly IBuildPipeConfigurator<PropertyInitializerContext<T, TProperty>> _configurator;
        readonly string _propertyName;

        public PropertyInitializerBuilder(string propertyName)
        {
            _propertyName = propertyName;
            _configurator = new PipeConfigurator<PropertyInitializerContext<T, TProperty>>();
        }

        public void Add(IPropertyInitializerFilter<T, TProperty> initializer)
        {
            _configurator.UseFilter(initializer);
        }

        public IPipe<InitializerContext<T>> Build()
        {
            var pipe = _configurator.Build();

            return new PropertyInitializerPipe<T, TProperty>(_propertyName, pipe);
        }
    }
}