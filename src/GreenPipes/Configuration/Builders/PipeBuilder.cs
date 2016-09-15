// Copyright 2013-2016 Chris Patterson
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
namespace GreenPipes.Builders
{
    using System.Collections.Generic;
    using Pipes;


    public class PipeBuilder<TContext> :
        IPipeBuilder<TContext>
        where TContext : class, PipeContext
    {
        readonly List<IFilter<TContext>> _filters;

        public PipeBuilder()
        {
            _filters = new List<IFilter<TContext>>();
        }

        public void AddFilter(IFilter<TContext> filter)
        {
            _filters.Add(filter);
        }

        public IPipe<TContext> Build()
        {
            if (_filters.Count == 0)
                return Pipe.Empty<TContext>();

            IPipe<TContext> current = new LastPipe<TContext>(_filters[_filters.Count - 1]);

            for (var i = _filters.Count - 2; i >= 0; i--)
                current = new FilterPipe<TContext>(_filters[i], current);

            return current;
        }
    }


    public class PipeBuilder<TContext, TResult> :
        IPipeBuilder<TContext, TResult>
        where TContext : class, PipeContext
        where TResult : class
    {
        readonly List<IFilter<TContext, TResult>> _filters;
        readonly IPipe<TContext, TResult> _handlerPipe;

        public PipeBuilder(IPipe<TContext, TResult> handlerPipe)
        {
            _handlerPipe = handlerPipe;
            _filters = new List<IFilter<TContext, TResult>>();
        }

        public void AddFilter(IFilter<TContext, TResult> filter)
        {
            _filters.Add(filter);
        }
        
        public IPipe<TContext, TResult> Build()
        {
            if (_filters.Count == 0)
                return new EmptyPipe<TContext, TResult>(_handlerPipe);

            IPipe<TContext, TResult> current = new LastPipe<TContext, TResult>(_filters[_filters.Count - 1], _handlerPipe);

            for (var i = _filters.Count - 2; i >= 0; i--)
                current = new FilterPipe<TContext, TResult>(_filters[i], current);

            return current;
        }
    }
}