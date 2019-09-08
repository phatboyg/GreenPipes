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
    using System.Threading.Tasks;
    using Internals.Extensions;
    using Observers;


    /// <summary>
    /// Converts an inbound context type to a pipe context type post-dispatch
    /// </summary>
    /// <typeparam name="TInput">The pipe context type</typeparam>
    /// <typeparam name="TOutput">The subsequent pipe context type</typeparam>
    public class OutputPipeFilter<TInput, TOutput> :
        IOutputPipeFilter<TInput, TOutput>
        where TInput : class, PipeContext
        where TOutput : class, TInput
    {
        readonly IPipeContextConverter<TInput, TOutput> _contextConverter;
        readonly FilterObservable<TOutput> _observers;
        readonly ITeeFilter<TOutput> _output;

        public OutputPipeFilter(IPipeContextConverter<TInput, TOutput> contextConverter, ITeeFilter<TOutput> outputFilter)
        {
            _contextConverter = contextConverter;

            _output = outputFilter;

            _observers = new FilterObservable<TOutput>();
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("dispatchPipe");
            scope.Add("outputType", TypeCache<TOutput>.ShortName);

            _output.Probe(scope);
        }

        Task IFilter<TInput>.Send(TInput context, IPipe<TInput> next)
        {
            return _contextConverter.TryConvert(context, out var pipeContext)
                ? SendToOutput(next, pipeContext)
                : next.Send(context);
        }

        async Task SendToOutput(IPipe<TInput> next, TOutput pipeContext)
        {
            if (_observers.Count > 0)
                await _observers.PreSend(pipeContext).ConfigureAwait(false);

            try
            {
                await _output.Send(pipeContext, next).ConfigureAwait(false);

                if (_observers.Count > 0)
                    await _observers.PostSend(pipeContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (_observers.Count > 0)
                    await _observers.SendFault(pipeContext, ex).ConfigureAwait(false);

                throw;
            }
        }

        ConnectHandle IObserverConnector<TOutput>.ConnectObserver(IFilterObserver<TOutput> observer)
        {
            return _observers.Connect(observer);
        }

        ConnectHandle IPipeConnector<TOutput>.ConnectPipe(IPipe<TOutput> pipe)
        {
            return _output.ConnectPipe(pipe);
        }
    }


    public class OutputPipeFilter<TInput, TOutput, TKey> :
        OutputPipeFilter<TInput, TOutput>,
        IOutputPipeFilter<TInput, TOutput, TKey>
        where TInput : class, PipeContext
        where TOutput : class, PipeContext, TInput
    {
        readonly ITeeFilter<TOutput, TKey> _outputFilter;

        public OutputPipeFilter(IPipeContextConverter<TInput, TOutput> contextConverter, KeyAccessor<TInput, TKey> keyAccessor)
            : this(contextConverter, new TeeFilter<TOutput, TKey>(keyAccessor))
        {
        }

        protected OutputPipeFilter(IPipeContextConverter<TInput, TOutput> contextConverter, ITeeFilter<TOutput, TKey> outputFilter)
            : base(contextConverter, outputFilter)
        {
            _outputFilter = outputFilter;
        }

        public ConnectHandle ConnectPipe<T>(TKey key, IPipe<T> pipe)
            where T : class, PipeContext
        {
            return _outputFilter.ConnectPipe(key, pipe);
        }
    }
}