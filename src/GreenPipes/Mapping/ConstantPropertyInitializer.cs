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
    using Util;


    /// <summary>
    /// Initialize an object property to a constant value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    public class ConstantPropertyInitializer<T, TProperty> :
        IPropertyInitializerFilter<T, TProperty>
        where T : class
    {
        readonly TProperty _propertyValue;

        public ConstantPropertyInitializer(TProperty propertyValue)
        {
            _propertyValue = propertyValue;
        }

        public Task Send(PropertyInitializerContext<T, TProperty> context, IPipe<PropertyInitializerContext<T, TProperty>> next)
        {
            context.PropertyValue = _propertyValue;

            return TaskUtil.Completed;
        }

        public void Probe(ProbeContext context)
        {
        }
    }
}