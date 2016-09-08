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
    using System.Threading.Tasks;


    /// <summary>
    /// A pipe filter which is correlated to a key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class KeyPipeFilter<T, TKey> :
        IFilter<T>
        where T : class, PipeContext
    {
        readonly TKey _key;
        readonly IPipe<T> _pipe;

        public KeyPipeFilter(TKey key, IPipe<T> pipe)
        {
            _key = key;
            _pipe = pipe;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("key");
            scope.Add("key", _key);
        }

        [DebuggerNonUserCode]
        public async Task Send(T context, IPipe<T> next)
        {
            await _pipe.Send(context).ConfigureAwait(false);

            await next.Send(context).ConfigureAwait(false);
        }
    }
}