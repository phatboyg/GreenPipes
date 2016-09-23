// Copyright 2012-2016 Chris Patterson
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
namespace GreenPipes.Observers
{
    using System.Threading.Tasks;
    using Util;


    public class RetryObservable :
        Connectable<IRetryObserver>,
        IRetryObserver
    {
        public Task PostCreate<T>(RetryPolicyContext<T> context)
            where T : class, PipeContext
        {
            return ForEachAsync(x => x.PostCreate(context));
        }

        public Task PostFault<T>(RetryContext<T> context)
            where T : class, PipeContext
        {
            return ForEachAsync(x => x.PostFault(context));
        }

        public Task PreRetry<T>(RetryContext<T> context)
            where T : class, PipeContext
        {
            return ForEachAsync(x => x.PreRetry(context));
        }

        public Task RetryFault<T>(RetryContext<T> context)
            where T : class, PipeContext
        {
            return ForEachAsync(x => x.RetryFault(context));
        }
    }
}