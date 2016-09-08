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
    /// <typeparam name="TKey"></typeparam>
    public class DispatchFilter<TInput, TKey> :
        IFilter<TInput>,
        IDispatchPipeConnector<TKey>,
        IDispatchObserverConnector<TKey>
        where TInput : class, PipeContext
    {
        readonly IPipeContextProviderFactory<TInput, TKey> _dispatchProvider;
        readonly FilterObservable _observers;
        readonly ConcurrentDictionary<TKey, IDispatchPipe> _pipes;

        public DispatchFilter(IPipeContextProviderFactory<TInput, TKey> dispatchProvider)
        {
            _dispatchProvider = dispatchProvider;
            _pipes = new ConcurrentDictionary<TKey, IDispatchPipe>();
            _observers = new FilterObservable();
        }

        ConnectHandle IDispatchObserverConnector<TKey>.ConnectObserver<T>(TKey key, IFilterObserver<T> observer)
        {
            return GetPipe<T>(key).ConnectObserver(observer);
        }

        public ConnectHandle ConnectPipe<T>(TKey key, IPipe<T> pipe)
            where T : class, PipeContext
        {
            if (pipe == null)
                throw new ArgumentNullException(nameof(pipe));

            IPipeConnector<T> messagePipe = GetPipe<T, IPipeConnector<T>>(key);

            return messagePipe.ConnectPipe(pipe);
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            foreach (var pipe in _pipes.Values)
                pipe.Filter.Probe(context);
        }

        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public Task Send(TInput context, IPipe<TInput> next)
        {
            return Task.WhenAll(_pipes.Values.Select(x => x.Filter.Send(context, next)));
        }

        TResult GetPipe<T, TResult>(TKey key)
            where T : class, PipeContext
            where TResult : class
        {
            return GetPipe<T>(key).As<TResult>();
        }

        IDispatchPipe GetPipe<T>(TKey key)
            where T : class, PipeContext
        {
            return _pipes.GetOrAdd(key, x => new DispatchPipe<T>(_dispatchProvider.GetPipeContextFactory<T>(key)));
        }

        public void AddFilter<T>(TKey key, IFilter<T> filter)
            where T : class, PipeContext
        {
            GetPipe<T>(key).AddFilter(filter);
        }


        interface IDispatchPipe :
            IObserverConnector
        {
            IFilter<TInput> Filter { get; }

            TResult As<TResult>()
                where TResult : class;

            void AddFilter<T>(IFilter<T> filter)
                where T : class, PipeContext;
        }


        class DispatchPipe<TPipe> :
            IDispatchPipe
            where TPipe : class, PipeContext
        {
            readonly IPipeContextProvider<TInput, TPipe> _contextProvider;
            readonly Lazy<DispatchPipeFilter<TInput, TPipe>> _filter;
            readonly IList<IFilter<TPipe>> _pipeFilters;

            public DispatchPipe(IPipeContextProvider<TInput, TPipe> contextProvider)
            {
                _filter = new Lazy<DispatchPipeFilter<TInput, TPipe>>(CreateFilter);
                _contextProvider = contextProvider;

                _pipeFilters = new List<IFilter<TPipe>>();
            }

            public IFilter<TInput> Filter => _filter.Value;

            TResult IDispatchPipe.As<TResult>()
            {
                return _filter.Value as TResult;
            }

            void IDispatchPipe.AddFilter<T>(IFilter<T> filter)
            {
                if (_filter.IsValueCreated)
                    throw new PipeConfigurationException("The filter has already been created, no additional filters can be added");

                var self = this as DispatchPipe<T>;
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

            DispatchPipeFilter<TInput, TPipe> CreateFilter()
            {
                return new DispatchPipeFilter<TInput, TPipe>(_pipeFilters, _contextProvider);
            }
        }
    }
}