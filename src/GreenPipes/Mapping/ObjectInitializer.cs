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
    using System.Threading;
    using System.Threading.Tasks;
    using Contexts;


    public class ObjectInitializer<TObject> :
        IObjectInitializer<TObject>
        where TObject : class
    {
        readonly IObjectFactory<TObject> _factory;
        readonly IFactoryInitializer<TObject>[] _factoryInitializers;
        readonly IPipe<InitializerContext<TObject>>[] _initializers;

        public ObjectInitializer(IObjectFactory<TObject> factory, IFactoryInitializer<TObject>[] factoryInitializers,
            IPipe<InitializerContext<TObject>>[] initializers)
        {
            _factory = factory;

            _factoryInitializers = factoryInitializers ?? new IFactoryInitializer<TObject>[0];
            _initializers = initializers ?? new IPipe<InitializerContext<TObject>>[0];
        }

        public async Task<TObject> Initialize(CancellationToken cancellationToken)
        {
            var pipeContext = new InternalPipeContext(cancellationToken);

            var context = Create(pipeContext);

            for (int i = 0; i < _initializers.Length; i++)
                await _initializers[i].Send(context).ConfigureAwait(false);

            return context.Object;
        }

        InitializerContext<TObject> Create(PipeContext context)
        {
            var factoryContext = _factory.Create(context);

            for (int i = 0; i < _factoryInitializers.Length; i++)
                _factoryInitializers[i].Initialize(factoryContext);

            return factoryContext;
        }
    }
}