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
namespace GreenPipes.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Observers;
    using Util;


    /// <summary>
    /// Dispatches an inbound pipe to one or more output pipes based on a dispatch
    /// type.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class DynamicFilter<TInput> :
        IDynamicFilter<TInput>
        where TInput : class, PipeContext
    {
        readonly Dictionary<Type, IOutputFilter> _outputPipes;
        readonly IPipe<TInput> _empty;
        protected readonly IPipeContextConverterFactory<TInput> ConverterFactory;
        protected readonly FilterObservable Observers;

        IOutputFilter[] _outputPipeArray;

        public DynamicFilter(IPipeContextConverterFactory<TInput> converterFactory)
        {
            ConverterFactory = converterFactory;

            _outputPipes = new Dictionary<Type, IOutputFilter>();
            _outputPipeArray = _outputPipes.Values.ToArray();

            Observers = new FilterObservable();
            _empty = Pipe.Empty<TInput>();
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
            foreach (var pipe in _outputPipes.Values)
                pipe.Probe(context);
        }

        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public Task Send(TInput context, IPipe<TInput> next)
        {
            var outputPipes = _outputPipeArray;

            if (outputPipes.Length == 1)
            {
                return outputPipes[0].Send(context, next);
            }

            if (outputPipes.Length > 1)
            {
                async Task SendAsync()
                {
                    var outputTasks = new Task[outputPipes.Length];
                    for (int i = 0; i < outputPipes.Length; i++)
                        outputTasks[i] = outputPipes[i].Send(context, _empty);

                    await Task.WhenAll(outputTasks).ConfigureAwait(false);
                    await next.Send(context).ConfigureAwait(false);
                }

                return SendAsync();
            }

            return TaskUtil.Completed;
        }

        protected TResult GetPipe<T, TResult>()
            where T : class, PipeContext
            where TResult : class
        {
            return GetPipe<T>().As<TResult>();
        }

        protected IOutputFilter GetPipe<T>()
            where T : class, PipeContext
        {
            lock (_outputPipes)
            {
                if (_outputPipes.TryGetValue(typeof(T), out var outputPipe))
                    return outputPipe;

                outputPipe = CreateOutputPipe<T>();

                _outputPipes.Add(typeof(T), outputPipe);

                _outputPipeArray = _outputPipes.Values.ToArray();

                return outputPipe;
            }
        }

        protected virtual IOutputFilter CreateOutputPipe<T>()
            where T : class, PipeContext
        {
            var converter = ConverterFactory.GetConverter<T>();

            return (IOutputFilter)Activator.CreateInstance(typeof(OutputFilter<>).MakeGenericType(typeof(TInput), typeof(T)), Observers, converter);
        }


        protected interface IOutputFilter :
            IFilter<TInput>,
            IObserverConnector
        {
            TResult As<TResult>()
                where TResult : class;
        }


        protected class OutputFilter<TOutput> :
            IOutputFilter
            where TOutput : class, TInput
        {
            protected readonly IPipeContextConverter<TInput, TOutput> ContextConverter;
            protected readonly FilterObservable Observers;
            IOutputPipeFilter<TInput, TOutput> _filter;

            public OutputFilter(FilterObservable observers, IPipeContextConverter<TInput, TOutput> contextConverter)
            {
                ContextConverter = contextConverter;
                Observers = observers;
            }

            TResult IOutputFilter.As<TResult>()
            {
                return Filter as TResult;
            }

            ConnectHandle IObserverConnector.ConnectObserver<T>(IFilterObserver<T> observer)
            {
                if (Filter is IObserverConnector<T> connector)
                    return connector.ConnectObserver(observer);

                throw new ArgumentException($"The filter is not of the specified type: {typeof(T).Name}", nameof(observer));
            }

            public ConnectHandle ConnectObserver(IFilterObserver observer)
            {
                return Observers.Connect(observer);
            }

            public Task Send(TInput context, IPipe<TInput> next)
            {
                return Filter.Send(context, next);
            }

            public void Probe(ProbeContext context)
            {
                Filter.Probe(context);
            }

            protected virtual IOutputPipeFilter<TInput, TOutput> Filter => _filter ?? (_filter = CreateFilter());

            IOutputPipeFilter<TInput, TOutput> CreateFilter()
            {
                IOutputPipeFilter<TInput, TOutput> filter = new OutputPipeFilter<TInput, TOutput>(ContextConverter, new TeeFilter<TOutput>());

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

        protected override IOutputFilter CreateOutputPipe<T>()
        {
            var dynamicType = typeof(KeyOutputFilter<>).MakeGenericType(typeof(TInput), typeof(TKey), typeof(T));

            var pipe = Activator.CreateInstance(dynamicType, Observers, ConverterFactory.GetConverter<T>(), _keyAccessor);

            return (IOutputFilter)pipe;
        }


        protected class KeyOutputFilter<TOutput> :
            OutputFilter<TOutput>
            where TOutput : class, TInput
        {
            readonly KeyAccessor<TInput, TKey> _keyAccessor;
            IOutputPipeFilter<TInput, TOutput, TKey> _filter;

            public KeyOutputFilter(FilterObservable observers, IPipeContextConverter<TInput, TOutput> contextConverter, KeyAccessor<TInput, TKey> keyAccessor)
                : base(observers, contextConverter)
            {
                _keyAccessor = keyAccessor;
            }

            protected override IOutputPipeFilter<TInput, TOutput> Filter => _filter ?? (_filter = CreateKeyFilter());

            IOutputPipeFilter<TInput, TOutput, TKey> CreateKeyFilter()
            {
                IOutputPipeFilter<TInput, TOutput, TKey> filter = new OutputPipeFilter<TInput, TOutput, TKey>(ContextConverter, _keyAccessor);

                filter.ConnectObserver(new ObservableAdapter<TOutput>(Observers));

                return filter;
            }
        }
    }
}