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
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;


    /// <summary>
    /// Uses a retry policy to handle exceptions, retrying the operation in according
    /// with the policy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RepeatFilter<T> :
        IFilter<T>
        where T : class, PipeContext
    {
        readonly CancellationToken _cancellationToken;

        public RepeatFilter(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            context.CreateFilterScope("repeat");
        }

        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public async Task Send(T context, IPipe<T> next)
        {
            while (_cancellationToken.IsCancellationRequested == false)
            {
                await next.Send(context).ConfigureAwait(false);
            }
        }
    }
}