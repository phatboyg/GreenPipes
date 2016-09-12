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
namespace GreenPipes.Pipes
{
    using System.Threading.Tasks;
    using Filters;


    /// <summary>
    /// A dynamic router is a pipe on which additional pipes can be connected and context is 
    /// routed through the pipe based upon the output requirements of the connected pipes. It is built
    /// around the dynamic filter, which is the central point of the router.
    /// </summary>
    public class DynamicRouter<TContext> :
        IDynamicRouter<TContext>
        where TContext : class, PipeContext
    {
        readonly IDynamicFilter<TContext> _filter;
        readonly IPipe<TContext> _pipe;

        public DynamicRouter(IPipeContextConverterFactory<TContext> converterFactory)
        {
            _filter = new DynamicFilter<TContext>(converterFactory);
            _pipe = Pipe.New<TContext>(x => x.UseFilter(_filter));
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateScope("dynamicRouter");

            _pipe.Probe(scope);
        }

        Task IPipe<TContext>.Send(TContext context)
        {
            return _pipe.Send(context);
        }

        public ConnectHandle ConnectPipe<T>(IPipe<T> pipe)
            where T : class, PipeContext
        {
            return _filter.ConnectPipe(pipe);
        }

        ConnectHandle IObserverConnector.ConnectObserver<T>(IFilterObserver<T> observer)
        {
            return _filter.ConnectObserver(observer);
        }

        ConnectHandle IObserverConnector.ConnectObserver(IFilterObserver observer)
        {
            return _filter.ConnectObserver(observer);
        }
    }


    public class DynamicRouter<TContext, TKey> :
        IDynamicRouter<TContext, TKey>
        where TContext : class, PipeContext
    {
        readonly IDynamicFilter<TContext, TKey> _filter;
        readonly IPipe<TContext> _pipe;

        public DynamicRouter(IPipeContextConverterFactory<TContext> converterFactory, KeyAccessor<TContext, TKey> keyAccessor)
        {
            _filter = new DynamicFilter<TContext, TKey>(converterFactory, keyAccessor);
            _pipe = Pipe.New<TContext>(x => x.UseFilter(_filter));
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateScope("dynamicRouter");

            _pipe.Probe(scope);
        }

        Task IPipe<TContext>.Send(TContext context)
        {
            return _pipe.Send(context);
        }

        public ConnectHandle ConnectPipe<T>(IPipe<T> pipe)
            where T : class, PipeContext
        {
            return _filter.ConnectPipe(pipe);
        }

        ConnectHandle IObserverConnector.ConnectObserver<T>(IFilterObserver<T> observer)
        {
            return _filter.ConnectObserver(observer);
        }

        ConnectHandle IObserverConnector.ConnectObserver(IFilterObserver observer)
        {
            return _filter.ConnectObserver(observer);
        }

        public ConnectHandle ConnectPipe<T>(TKey key, IPipe<T> pipe)
            where T : class, PipeContext
        {
            return _filter.ConnectPipe(key, pipe);
        }
    }
}