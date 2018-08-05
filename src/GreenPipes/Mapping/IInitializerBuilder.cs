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
    public interface IInitializerBuilder<TObject>
        where TObject : class
    {
        /// <summary>
        /// Add a simple property initializer, no checking is done to see what is actually initialized. Mainly for internal object setup.
        /// </summary>
        /// <param name="initializer"></param>
        void Add(IFactoryInitializer<TObject> initializer);

        /// <summary>
        /// Returns the property builder for the specified property, allowing property initializers to be added
        /// </summary>
        /// <param name="propertyName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPropertyInitializerBuilder<TObject, T> Property<T>(string propertyName);
    }
}