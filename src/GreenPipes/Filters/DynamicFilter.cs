// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace GreenPipes.Filters
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;


    /// <summary>
    /// Dispatches an inbound pipe to one or more output pipes based on a dispatch
    /// type.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class DynamicFilter<TInput> :
        IDynamicFilter<TInput>
        where TInput : class, PipeContext
    {
        protected readonly IPipeContextConverterFactory<TInput> ConverterFactory;
        protected readonly FilterObservable Observers;
        protected readonly ConcurrentDictionary<Type, IOutputPipe> OutputPipes;

        public DynamicFilter(IPipeContextConverterFactory<TInput> converterFactory)
        {
            ConverterFactory = converterFactory;
            OutputPipes = new ConcurrentDictionary<Type, IOutputPipe>();
            Observers = new FilterObservable();
        }

        ConnectHandle IObserverConnector.ConnectObserver<T>(IFilterObserver<T> observer)
        {
            return GetPipe<T>().ConnectObserver(observer);
        }

        ConnectHandle IObserverConnector.ConnectObserver(IFilterObserver observer)
        {
            return Observers.Connect(observer);
        }

        public ConnectHandle ConnectPipe<T>(IPipe<T> pipe)
            where T : class, PipeContext
        {
            if (pipe == null)
                throw new ArgumentNullException(nameof(pipe));

            IPipeConnector<T> pipeConnector = GetPipe<T, IPipeConnector<T>>();

            return pipeConnector.ConnectPipe(pipe);
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            foreach (var pipe in OutputPipes.Values)
                pipe.Filter.Probe(context);
        }

        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public Task Send(TInput context, IPipe<TInput> next)
        {
            return Task.WhenAll(OutputPipes.Values.Select(x => x.Filter.Send(context, next)));
        }

        protected TResult GetPipe<T, TResult>()
            where T : class, PipeContext
            where TResult : class
        {
            return GetPipe<T>().As<TResult>();
        }

        protected virtual IOutputPipe GetPipe<T>()
            where T : class, PipeContext
        {
            return OutputPipes.GetOrAdd(typeof(T), x => new OutputPipe<T>(Observers, ConverterFactory.GetConverter<T>()));
        }

        public void AddFilter<T>(IFilter<T> filter)
            where T : class, PipeContext
        {
            GetPipe<T>().AddFilter(filter);
        }


        protected interface IOutputPipe :
            IObserverConnector
        {
            IFilter<TInput> Filter { get; }

            TResult As<TResult>()
                where TResult : class;

            void AddFilter<T>(IFilter<T> filter)
                where T : class, PipeContext;
        }


        protected class OutputPipe<TOutput> :
            IOutputPipe
            where TOutput : class, PipeContext
        {
            readonly Lazy<IOutputPipeFilter<TInput, TOutput>> _filter;
            protected readonly IPipeContextConverter<TInput, TOutput> ContextConverter;
            protected readonly FilterObservable Observers;

            public OutputPipe(FilterObservable observers, IPipeContextConverter<TInput, TOutput> contextConverter)
            {
                _filter = new Lazy<IOutputPipeFilter<TInput, TOutput>>(CreateFilter);
                ContextConverter = contextConverter;
                Observers = observers;

                PipeFilters = new List<IFilter<TOutput>>();
            }

            public IList<IFilter<TOutput>> PipeFilters { get; }

            public IFilter<TInput> Filter => _filter.Value;

            TResult IOutputPipe.As<TResult>()
            {
                return _filter.Value as TResult;
            }

            void IOutputPipe.AddFilter<T>(IFilter<T> filter)
            {
                if (_filter.IsValueCreated)
                    throw new PipeConfigurationException("The filter has already been created, no additional filters can be added");

                var self = this as OutputPipe<T>;
                if (self == null)
                    throw new ArgumentException("The message type is invalid: " + typeof(T).Name);

                self.PipeFilters.Add(filter);
            }

            ConnectHandle IObserverConnector.ConnectObserver<T>(IFilterObserver<T> observer)
            {
                var connector = _filter.Value as IObserverConnector<T>;
                if (connector == null)
                    throw new ArgumentException($"The filter is not of the specified type: {typeof(T).Name}", nameof(observer));

                return connector.ConnectObserver(observer);
            }

            public ConnectHandle ConnectObserver(IFilterObserver observer)
            {
                return Observers.Connect(observer);
            }

            protected virtual IOutputPipeFilter<TInput, TOutput> CreateFilter()
            {
                IOutputPipeFilter<TInput, TOutput> filter = new OutputPipeFilter<TInput, TOutput>(PipeFilters, ContextConverter, new TeeFilter<TOutput>());

                filter.ConnectObserver(new ObservableAdapter<TOutput>(Observers));

                return filter;
            }
        }
    }


    public class DynamicFilter<TInput, TKey> :
        DynamicFilter<TInput>,
        IDynamicFilter<TInput, TKey>
        where TInput : class, PipeContext
    {
        readonly KeyAccessor<TInput, TKey> _keyAccessor;

        public DynamicFilter(IPipeContextConverterFactory<TInput> converterFactory, KeyAccessor<TInput, TKey> keyAccessor)
            : base(converterFactory)
        {
            _keyAccessor = keyAccessor;
        }

        public ConnectHandle ConnectPipe<T>(TKey key, IPipe<T> pipe)
            where T : class, PipeContext
        {
            if (pipe == null)
                throw new ArgumentNullException(nameof(pipe));

            IKeyPipeConnector<TKey> pipeConnector = GetPipe<T, IKeyPipeConnector<TKey>>();

            return pipeConnector.ConnectPipe(key, pipe);
        }

        protected override IOutputPipe GetPipe<T>()
        {
            return OutputPipes.GetOrAdd(typeof(T), x =>
            {
                var dynamicType = typeof(KeyOutputPipe<>).MakeGenericType(typeof(TInput), typeof(TKey), typeof(T));

                var pipe = Activator.CreateInstance(dynamicType, Observers, ConverterFactory.GetConverter<T>(), _keyAccessor);

                return (IOutputPipe)pipe;
            });
        }


        protected class KeyOutputPipe<TOutput> :
            OutputPipe<TOutput>
            where TOutput : class, TInput
        {
            readonly Lazy<IOutputPipeFilter<TInput, TOutput, TKey>> _filter;
            readonly KeyAccessor<TInput, TKey> _keyAccessor;

            public KeyOutputPipe(FilterObservable observers, IPipeContextConverter<TInput, TOutput> contextConverter, KeyAccessor<TInput, TKey> keyAccessor)
                : base(observers, contextConverter)
            {
                _keyAccessor = keyAccessor;

                _filter = new Lazy<IOutputPipeFilter<TInput, TOutput, TKey>>(CreateKeyFilter);
            }

            protected override IOutputPipeFilter<TInput, TOutput> CreateFilter()
            {
                return _filter.Value;
            }

            IOutputPipeFilter<TInput, TOutput, TKey> CreateKeyFilter()
            {
                IOutputPipeFilter<TInput, TOutput, TKey> filter = new OutputPipeFilter<TInput, TOutput, TKey>(PipeFilters, ContextConverter, _keyAccessor);

                filter.ConnectObserver(new ObservableAdapter<TOutput>(Observers));

                return filter;
            }
        }
    }
}