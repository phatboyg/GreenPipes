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
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;


    public static class TaskExtensions
    {
        public static Task TrySetResultOnThreadPool<T>(this TaskCompletionSource<T> source, T result)
        {
            void SetResult() => source.TrySetResult(result);

            return Task.Run(SetResult);
        }

        public static Task TrySetExceptionOnThreadPool<T>(this TaskCompletionSource<T> source, Exception exception)
        {
            void SetException() => source.TrySetException(exception);

            return Task.Run(SetException);
        }

        public static Task TrySetCanceledOnThreadPool<T>(this TaskCompletionSource<T> source)
        {
            void SetCanceled() => source.TrySetCanceled();

            return Task.Run(SetCanceled);
        }

        public static Task OrCanceled(this Task task, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
                return task;

            async Task WaitAsync()
            {
                var source = new TaskCompletionSource<bool>();
                using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), source))
                {
                    var completed = await Task.WhenAny(task, source.Task).ConfigureAwait(false);
                    if (completed != task)
                        throw new OperationCanceledException(cancellationToken);

                    task.GetAwaiter().GetResult();
                }
            }

            return WaitAsync();
        }

        public static Task<T> OrCanceled<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
                return task;

            async Task<T> WaitAsync()
            {
                var source = new TaskCompletionSource<bool>();
                using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), source))
                {
                    var completed = await Task.WhenAny(task, source.Task).ConfigureAwait(false);
                    if (completed != task)
                        throw new OperationCanceledException(cancellationToken);

                    return task.GetAwaiter().GetResult();
                }
            }

            return WaitAsync();
        }

        static readonly TimeSpan _defaultTimeout = new TimeSpan(0, 0, 0, 5, 0);

        public static Task OrTimeout(this Task task, int d = 0, int h = 0, int m = 0, int s = 0, int ms = 0, CancellationToken cancellationToken = default,
            [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int? lineNumber = null)
        {
            var timeout = new TimeSpan(d, h, m, s, ms);
            if (timeout == TimeSpan.Zero)
                timeout = _defaultTimeout;

            return OrTimeoutInternal(task, timeout, cancellationToken, memberName, filePath, lineNumber);
        }

        public static Task OrTimeout(this Task task, TimeSpan timeout, CancellationToken cancellationToken = default,
            [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int? lineNumber = null)
        {
            return OrTimeoutInternal(task, timeout, cancellationToken, memberName, filePath, lineNumber);
        }

        static Task OrTimeoutInternal(this Task task, TimeSpan timeout, CancellationToken cancellationToken, string memberName, string filePath,
            int? lineNumber)
        {
            if (task.IsCompleted)
                return task;

            async Task WaitAsync()
            {
                var completed = await Task.WhenAny(task, Task.Delay(Debugger.IsAttached ? Timeout.InfiniteTimeSpan : timeout, cancellationToken))
                    .ConfigureAwait(false);
                if (completed != task)
                    throw new TimeoutException(FormatTimeoutMessage(memberName, filePath, lineNumber));

                task.GetAwaiter().GetResult();
            }

            return WaitAsync();
        }

        public static Task<T> OrTimeout<T>(this Task<T> task, int d = 0, int h = 0, int m = 0, int s = 0, int ms = 0,
            CancellationToken cancellationToken = default,
            [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null,
            [CallerLineNumber] int? lineNumber = null)
        {
            var timeout = new TimeSpan(d, h, m, s, ms);
            if (timeout == TimeSpan.Zero)
                timeout = _defaultTimeout;

            return OrTimeoutInternal(task, timeout, cancellationToken, memberName, filePath, lineNumber);
        }

        public static Task<T> OrTimeout<T>(this Task<T> task, TimeSpan timeout, CancellationToken cancellationToken = default,
            [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int? lineNumber = null)
        {
            return OrTimeoutInternal(task, timeout, cancellationToken, memberName, filePath, lineNumber);
        }

        static Task<T> OrTimeoutInternal<T>(this Task<T> task, TimeSpan timeout, CancellationToken cancellationToken, string memberName, string filePath,
            int? lineNumber)
        {
            if (task.IsCompleted)
                return task;

            async Task<T> WaitAsync()
            {
                var completed = await Task.WhenAny(task, Task.Delay(Debugger.IsAttached ? Timeout.InfiniteTimeSpan : timeout, cancellationToken))
                    .ConfigureAwait(false);
                if (completed != task)
                    throw new TimeoutException(FormatTimeoutMessage(memberName, filePath, lineNumber));

                return task.GetAwaiter().GetResult();
            }

            return WaitAsync();
        }

        static string FormatTimeoutMessage(string memberName, string filePath, int? lineNumber)
        {
            return !string.IsNullOrEmpty(memberName)
                ? $"Operation in {memberName} timed out at {filePath}:{lineNumber}"
                : "Operation timed out";
        }

        /// <summary>
        /// Returns true if a Task was ran to completion (without being cancelled or faulted)
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static bool IsCompletedSuccessfully(this Task task)
        {
            return task.Status == TaskStatus.RanToCompletion;
        }
    }
}
