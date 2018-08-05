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
    /// <summary>
    /// Factory for the dynamic type created to implement the interface.
    /// </summary>
    /// <typeparam name="T">The interface type</typeparam>
    /// <typeparam name="TImplementation">The implementation type, which is dynamically created</typeparam>
    public class DefaultConstructorObjectFactory<T, TImplementation> :
        IObjectFactory<T>
        where T : class
        where TImplementation : T, new()
    {
        readonly IFactoryInitializer<T>[] _initializers;

        public DefaultConstructorObjectFactory(params IFactoryInitializer<T>[] initializers)
        {
            _initializers = initializers ?? new IFactoryInitializer<T>[0];
        }

        public InitializerContext<T> Create(PipeContext context)
        {
            var value = new TImplementation();

            var factoryContext = new DynamicFactoryContext<T>(context, value);

            for (int i = 0; i < _initializers.Length; i++)
                _initializers[i].Initialize(factoryContext);

            return factoryContext;
        }
    }
}