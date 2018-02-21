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


    /// <summary>
    /// A stack-based pipe used to insert filters into the pipeline without breaking up the order of the pipe
    /// delivery
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct PushPipe<T> :
        IPipe<T>
        where T : class, PipeContext
    {
        readonly IFilter<T> _filter;
        readonly IPipe<T> _next;

        public PushPipe(IFilter<T> filter, IPipe<T> next)
        {
            _filter = filter;
            _next = next;
        }

        Task IPipe<T>.Send(T context)
        {
            return _filter.Send(context, _next);
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            _filter.Probe(context);

            _next.Probe(context);
        }
    }
}