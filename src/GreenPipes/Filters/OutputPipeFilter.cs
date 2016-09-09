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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Internals.Extensions;
    using Pipes;


    /// <summary>
    /// Converts an inbound context type to a pipe context type post-dispatch
    /// </summary>
    /// <typeparam name="TInput">The pipe context type</typeparam>
    /// <typeparam name="TOutput">The subsequent pipe context type</typeparam>
    public class OutputPipeFilter<TInput, TOutput> :
        IOutputPipeFilter<TInput, TOutput>
        where TInput : class, PipeContext
        where TOutput : class, PipeContext
    {
        readonly IPipeContextConverter<TInput, TOutput> _contextConverter;
        readonly FilterObservable<TOutput> _observers;
        readonly ITeeFilter<TOutput> _output;
        readonly IPipe<TOutput> _outputPipe;

        public OutputPipeFilter(IEnumerable<IFilter<TOutput>> filters, IPipeContextConverter<TInput, TOutput> contextConverter,
            ITeeFilter<TOutput> outputFilter)
        {
            _contextConverter = contextConverter;

            _output = outputFilter;

            _outputPipe = BuildOutputPipe(filters.Concat(Enumerable.Repeat(_output, 1)).ToArray());

            _observers = new FilterObservable<TOutput>();
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("dispatchPipe");
            scope.Add("outputType", TypeNameCache<TOutput>.ShortName);

            _outputPipe.Probe(scope);
        }

        [DebuggerNonUserCode]
        async Task IFilter<TInput>.Send(TInput context, IPipe<TInput> next)
        {
            TOutput pipeContext;
            if (_contextConverter.TryConvert(context, out pipeContext))
            {
                if (_observers.Count > 0)
                    await _observers.PreSend(pipeContext).ConfigureAwait(false);
                try
                {
                    await _outputPipe.Send(pipeContext).ConfigureAwait(false);

                    if (_observers.Count > 0)
                        await _observers.PostSend(pipeContext).ConfigureAwait(false);

                    await next.Send(context).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // we can't await in a catch block, so we have to wait explicitly on this one
                    if (_observers.Count > 0)
                    {
                        await _observers.SendFault(pipeContext, ex).ConfigureAwait(false);
                    }

                    throw;
                }
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

        static IPipe<TOutput> BuildOutputPipe(IFilter<TOutput>[] filters)
        {
            if (filters.Length == 0)
                throw new ArgumentException("There must be at least one filter, the output filter, for the output pipe");

            IPipe<TOutput> current = new LastPipe<TOutput>(filters[filters.Length - 1]);

            for (var i = filters.Length - 2; i >= 0; i--)
                current = new FilterPipe<TOutput>(filters[i], current);

            return current;
        }
    }


    public class OutputPipeFilter<TInput, TOutput, TKey> :
        OutputPipeFilter<TInput, TOutput>,
        IOutputPipeFilter<TInput, TOutput, TKey>
        where TInput : class, PipeContext
        where TOutput : class, PipeContext
    {
        readonly ITeeFilter<TOutput, TKey> _outputFilter;

        public OutputPipeFilter(IEnumerable<IFilter<TOutput>> filters, IPipeContextConverter<TInput, TOutput> contextConverter,
            KeyAccessor<TOutput, TKey> keyAccessor)
            : this(filters, contextConverter, new TeeFilter<TOutput, TKey>(keyAccessor))
        {
        }

        protected OutputPipeFilter(IEnumerable<IFilter<TOutput>> filters, IPipeContextConverter<TInput, TOutput> contextConverter,
            ITeeFilter<TOutput, TKey> outputFilter)
            : base(filters, contextConverter, outputFilter)
        {
            _outputFilter = outputFilter;
        }

        public ConnectHandle ConnectPipe(TKey key, IPipe<TOutput> pipe)
        {
            return _outputFilter.ConnectPipe(key, pipe);
        }
    }
}