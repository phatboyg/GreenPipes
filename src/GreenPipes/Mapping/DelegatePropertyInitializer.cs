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
    using System.Threading.Tasks;


    /// <summary>
    /// Uses a delegate to return a property value, which is then used to set the property's value
    /// </summary>
    /// <typeparam name="T">The object type</typeparam>
    /// <typeparam name="TProperty">The property type</typeparam>
    public class DelegatePropertyInitializer<T, TProperty> :
        IPropertyInitializerFilter<T, TProperty>
        where T : class
    {
        readonly PropertyValueProvider<T, TProperty> _provider;

        public DelegatePropertyInitializer(PropertyValueProvider<T, TProperty> provider)
        {
            _provider = provider;
        }

        public async Task Send(PropertyInitializerContext<T, TProperty> context, IPipe<PropertyInitializerContext<T, TProperty>> next)
        {
            var propertyValue = await _provider(context).ConfigureAwait(false);

            context.PropertyValue = propertyValue;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
        }
    }
}