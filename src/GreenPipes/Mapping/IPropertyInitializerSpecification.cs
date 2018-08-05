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


    public interface IPropertyInitializerSpecification<T, TProperty> :
        ISpecification
        where T : class
    {
        /// <summary>
        /// Return the types referenced by this initializer, which may include nested/child object types
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type> GetReferencedTypes();

        void Apply(IPropertyInitializerBuilder<T, TProperty> builder);
    }
}