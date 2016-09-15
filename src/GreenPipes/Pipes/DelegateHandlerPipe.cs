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
namespace GreenPipes.Pipes
{
    using System.Threading.Tasks;


    /// <summary>
    /// A handler for a context with a result
    /// </summary>
    public class DelegateHandlerPipe<TContext, TResult> :
        IPipe<TContext, TResult>
        where TContext : class, PipeContext
        where TResult : class
    {
        readonly ContextHandler<TContext, TResult> _handler;

        public DelegateHandlerPipe(ContextHandler<TContext, TResult> handler)
        {
            _handler = handler;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("handler");
        }

        public Task<TResult> Send(TContext context)
        {
            return _handler(context);
        }
    }
}