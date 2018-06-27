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
namespace GreenPipes.Pipes
{
    using System.Threading.Tasks;
    using Mapping;


    public class DefaultResultPipe<TContext, TResult> :
        IResultPipe<TContext, TResult>
        where TContext : class, PipeContext
    {
        readonly Task<TResult> _result;

        public DefaultResultPipe()
        {
            _result = Task.FromResult(default(TResult));
        }

        Task<TResult> IResultPipe<TContext, TResult>.Send(TContext context)
        {
            return _result;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
        }
    }
}