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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Pipes;


    /// <summary>
    /// A filter that hold a place in the pipe so that future filters can be added at this position.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PushFilter<T> :
        IFilter<T>,
        IPushFilter<T>
        where T : class, PipeContext
    {
        readonly IList<IFilter<T>> _filters;

        public PushFilter()
        {
            _filters = new List<IFilter<T>>(1);
        }

        Task IFilter<T>.Send(T context, IPipe<T> next)
        {
            if (_filters.Count == 0)
                return next.Send(context);

            IPipe<T> pipe = next;
            for (int i = _filters.Count - 1; i >= 0; i--)
            {
                pipe = new PushPipe<T>(_filters[i], pipe);
            }

            return pipe.Send(context);
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            for (int i = 0; i < _filters.Count; i++)
            {
                _filters[i].Probe(context);
            }
        }

        public void Add(IFilter<T> filter)
        {
            _filters.Add(filter);
        }
    }
}