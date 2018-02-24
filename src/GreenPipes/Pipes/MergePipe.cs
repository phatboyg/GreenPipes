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
    using Internals.Extensions;


    public class MergePipe<TInput, TSplit> :
        IPipe<TSplit>
        where TSplit : class, PipeContext
        where TInput : class, PipeContext
    {
        readonly MergeFilterContextProvider<TInput, TSplit> _contextProvider;
        readonly TInput _input;
        readonly IPipe<TInput> _next;

        public MergePipe(IPipe<TInput> next, TInput input, MergeFilterContextProvider<TInput, TSplit> contextProvider)
        {
            _next = next;
            _input = input;
            _contextProvider = contextProvider;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("merge");
            scope.Set(new
            {
                InputType = TypeCache<TInput>.ShortName
            });

            _next.Probe(scope);
        }

        public Task Send(TSplit context)
        {
            var inputContext = _contextProvider(_input, context);

            return _next.Send(inputContext);
        }
    }
}