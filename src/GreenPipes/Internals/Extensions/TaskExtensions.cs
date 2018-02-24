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
namespace GreenPipes.Internals.Extensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;


    public static class TaskExtensions
    {
        /// <summary>
        /// Returns a Task that is either the completed task or an OperationCancelledException waiting for the Task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static async Task<T> UntilCompletedOrCanceled<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
                    throw new OperationCanceledException(cancellationToken);
            }

            return await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a Task that is either the completed task or an OperationCancelledException waiting for the Task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static async Task UntilCompletedOrCanceled(this Task task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
                    throw new OperationCanceledException(cancellationToken);
            }

            await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a Task that is either the completed task or a TimeoutException
        /// </summary>
        /// <param name="task"></param>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static async Task UntilCompletedOrTimeout(this Task task, int milliseconds)
        {
            using (var tokenSource = new CancellationTokenSource(milliseconds))
            {
                var tcs = new TaskCompletionSource<bool>();
                using (tokenSource.Token.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                {
                    if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
                        throw new TimeoutException("Timeout waiting for Task completion");
                }

                await task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Returns a Task that is either the completed task or a TimeoutException
        /// </summary>
        /// <param name="task"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static async Task UntilCompletedOrTimeout(this Task task, TimeSpan timeout)
        {
            using (var tokenSource = new CancellationTokenSource(timeout))
            {
                var tcs = new TaskCompletionSource<bool>();
                using (tokenSource.Token.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                {
                    if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
                        throw new TimeoutException("Timeout waiting for Task completion");
                }

                await task.ConfigureAwait(false);
            }
        }
    }
}