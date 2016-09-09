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
        readonly IPipeContextConverterFactory<TInput> _converterFactory;
        readonly FilterObservable _observers;
        readonly ConcurrentDictionary<Type, IOutputPipe> _outputPipes;

        public DynamicFilter(IPipeContextConverterFactory<TInput> converterFactory)
        {
            _converterFactory = converterFactory;
            _outputPipes = new ConcurrentDictionary<Type, IOutputPipe>();
            _observers = new FilterObservable();
        }

        ConnectHandle IObserverConnector.ConnectObserver<T>(IFilterObserver<T> observer)
        {
            return GetPipe<T>().ConnectObserver(observer);
        }

        ConnectHandle IObserverConnector.ConnectObserver(IFilterObserver observer)
        {
            return _observers.Connect(observer);
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
            foreach (var pipe in _outputPipes.Values)
                pipe.Filter.Probe(context);
        }

        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public Task Send(TInput context, IPipe<TInput> next)
        {
            return Task.WhenAll(_outputPipes.Values.Select(x => x.Filter.Send(context, next)));
        }

        TResult GetPipe<T, TResult>()
            where T : class, PipeContext
            where TResult : class
        {
            return GetPipe<T>().As<TResult>();
        }

        IOutputPipe GetPipe<T>()
            where T : class, PipeContext
        {
            return _outputPipes.GetOrAdd(typeof(T), x => new OutputPipe<T>(_observers, _converterFactory.GetConverter<T>()));
        }

        public void AddFilter<T>(IFilter<T> filter)
            where T : class, PipeContext
        {
            GetPipe<T>().AddFilter(filter);
        }


        interface IOutputPipe :
            IObserverConnector
        {
            IFilter<TInput> Filter { get; }

            TResult As<TResult>()
                where TResult : class;

            void AddFilter<T>(IFilter<T> filter)
                where T : class, PipeContext;
        }


        class OutputPipe<TOutput> :
            IOutputPipe
            where TOutput : class, PipeContext
        {
            readonly IPipeContextConverter<TInput, TOutput> _contextConverter;
            readonly Lazy<IOutputPipeFilter<TInput, TOutput>> _filter;
            readonly FilterObservable _observers;
            readonly IList<IFilter<TOutput>> _pipeFilters;

            public OutputPipe(FilterObservable observers, IPipeContextConverter<TInput, TOutput> contextConverter)
            {
                _filter = new Lazy<IOutputPipeFilter<TInput, TOutput>>(CreateFilter);
                _contextConverter = contextConverter;
                _observers = observers;

                _pipeFilters = new List<IFilter<TOutput>>();
            }

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

                self._pipeFilters.Add(filter);
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
                return _observers.Connect(observer);
            }

            IOutputPipeFilter<TInput, TOutput> CreateFilter()
            {
                IOutputPipeFilter<TInput, TOutput> filter = new OutputPipeFilter<TInput, TOutput>(_pipeFilters, _contextConverter, new TeeFilter<TOutput>());

                filter.ConnectObserver(new ObservableAdapter<TOutput>(_observers));

                return filter;
            }
        }
    }
}