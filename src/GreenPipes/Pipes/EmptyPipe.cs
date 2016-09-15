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
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Util;


    public class EmptyPipe<TContext> :
        IPipe<TContext>
        where TContext : class, PipeContext
    {
        [DebuggerNonUserCode]
        public Task Send(TContext context)
        {
            return TaskUtil.Completed;
        }

        public void Probe(ProbeContext context)
        {
        }
    }


    public class EmptyPipe<TContext, TResult> :
        IPipe<TContext, TResult>
        where TContext : class, PipeContext
        where TResult : class
    {
        readonly IPipe<TContext, TResult> _handlerPipe;

        public EmptyPipe(IPipe<TContext, TResult> handlerPipe)
        {
            _handlerPipe = handlerPipe;
        }

        [DebuggerNonUserCode]
        public Task<TResult> Send(TContext context)
        {
            return _handlerPipe.Send(context);
        }

        public void Probe(ProbeContext context)
        {
            _handlerPipe.Probe(context);
        }
    }
}