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
    using Internals.Reflection;


    /// <summary>
    /// Maps the factory context to the property context, and calls the initializer pipe
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    public class PropertyInitializerPipe<T, TProperty> :
        IPipe<InitializerContext<T>>
        where T : class
    {
        readonly IPipe<PropertyInitializerContext<T, TProperty>> _pipe;
        readonly IWriteProperty<T, TProperty> _writeProperty;

        public PropertyInitializerPipe(string propertyName, IPipe<PropertyInitializerContext<T, TProperty>> pipe)
        {
            _pipe = pipe;

            _writeProperty = WritePropertyCache<T>.GetProperty<TProperty>(propertyName);
        }

        public Task Send(InitializerContext<T> context)
        {
            var propertyContext = new ObjectPropertyInitializerContext<T, TProperty>(context, _writeProperty);

            return _pipe.Send(propertyContext);
        }

        public void Probe(ProbeContext context)
        {
            _pipe.Probe(context);
        }
    }
}