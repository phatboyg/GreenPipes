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
    using System;
    using System.Threading.Tasks;
    using Util;


    /// <summary>
    /// An AgentContext which arrives in the future, via async delivery
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncAgentContext<T> :
        IAsyncAgentContext<T>
        where T : class, PipeContext
    {
        readonly TaskCompletionSource<T> _context;
        readonly TaskCompletionSource<DateTime> _inactive;

        public AsyncAgentContext()
        {
            _context = new TaskCompletionSource<T>();
            _inactive = new TaskCompletionSource<DateTime>();
        }

        bool AgentContextHandle<T>.IsInactive => _inactive.Task.Status != TaskStatus.WaitingForActivation;

        Task<T> AgentContextHandle<T>.Context => _context.Task;

        Task AgentContextHandle<T>.Disavow()
        {
            _inactive.TrySetResult(DateTime.UtcNow);

            return TaskUtil.Completed;
        }

        Task IAsyncAgentContext<T>.Created(T context)
        {
            _context.SetResult(context);

            return TaskUtil.Completed;
        }

        Task IAsyncAgentContext<T>.CreateFailed(Exception exception)
        {
            _context.SetException(exception);

            return TaskUtil.Completed;
        }

        Task IAsyncAgentContext<T>.Faulted(Exception exception)
        {
            _inactive.TrySetException(exception);

            return TaskUtil.Completed;
        }
    }
}