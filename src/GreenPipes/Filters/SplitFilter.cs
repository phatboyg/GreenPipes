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
    using Internals.Extensions;
    using Pipes;


    public class SplitFilter<TInput, TSplit> :
        IFilter<TInput>
        where TSplit : class, PipeContext
        where TInput : class, PipeContext
    {
        readonly MergeFilterContextProvider<TInput, TSplit> _contextProvider;
        readonly FilterContextProvider<TSplit, TInput> _inputContextProvider;
        readonly IFilter<TSplit> _split;

        public SplitFilter(IFilter<TSplit> split, MergeFilterContextProvider<TInput, TSplit> contextProvider,
            FilterContextProvider<TSplit, TInput> inputContextProvider)
        {
            _split = split;
            _contextProvider = contextProvider;
            _inputContextProvider = inputContextProvider;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("split");
            scope.Set(new
            {
                SplitType = TypeNameCache<TSplit>.ShortName
            });

            _split.Probe(scope);
        }

        [DebuggerNonUserCode]
        public Task Send(TInput context, IPipe<TInput> next)
        {
            var mergePipe = new MergePipe<TInput, TSplit>(next, context, _contextProvider);

            return _split.Send(_inputContextProvider(context), mergePipe);
        }
    }
}