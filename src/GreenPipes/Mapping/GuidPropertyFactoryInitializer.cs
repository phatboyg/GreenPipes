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
    using Internals.Reflection;


    /// <summary>
    /// Initialize an object property to a constant value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GuidPropertyFactoryInitializer<T> :
        IFactoryInitializer<T>
        where T : class
    {
        readonly IWriteProperty<T, Guid> _writeProperty;

        public GuidPropertyFactoryInitializer(string propertyName)
        {
            _writeProperty = WritePropertyCache<T>.GetProperty<Guid>(propertyName);
        }

        public void Initialize(InitializerContext<T> context)
        {
            var propertyValue = Guid.NewGuid();

            _writeProperty.Set(context.Object, propertyValue);
        }
    }
}