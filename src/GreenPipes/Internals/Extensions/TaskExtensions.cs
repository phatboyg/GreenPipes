namespace GreenPipes.Internals.Extensions
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Util;


    public static class TaskExtensions
    {
        static readonly TimeSpan _defaultTimeout = new TimeSpan(0, 0, 0, 5, 0);

        public static Task TrySetResultOnThreadPool<T>(this TaskCompletionSource<T> source, T result)
        {
            return Task.Run(() => source.TrySetResult(result));
        }

        public static Task TrySetExceptionOnThreadPool<T>(this TaskCompletionSource<T> source, Exception exception)
        {
            return Task.Run(() => source.TrySetException(exception));
        }

        public static Task TrySetCanceledOnThreadPool<T>(this TaskCompletionSource<T> source)
        {
            return Task.Run(() => source.TrySetCanceled());
        }

        public static Task OrCanceled(this Task task, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
                return task;

            async Task WaitAsync()
            {
                using (cancellationToken.RegisterTask(out var cancelTask))
                {
                    var completed = await Task.WhenAny(task, cancelTask).ConfigureAwait(false);
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
                using (cancellationToken.RegisterTask(out var cancelTask))
                {
                    var completed = await Task.WhenAny(task, cancelTask).ConfigureAwait(false);
                    if (completed != task)
                        throw new OperationCanceledException(cancellationToken);

                    return task.GetAwaiter().GetResult();
                }
            }

            return WaitAsync();
        }

        public static Task OrTimeout(this Task task, int ms = 0, int s = 0, int m = 0, int h = 0, int d = 0, CancellationToken cancellationToken = default,
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
                var cancel = new CancellationTokenSource();

                var registration = cancellationToken.RegisterIfCanBeCanceled(cancel);
                try
                {
                    var delayTask = Task.Delay(Debugger.IsAttached ? Timeout.InfiniteTimeSpan : timeout, cancel.Token);

                    var completed = await Task.WhenAny(task, delayTask).ConfigureAwait(false);
                    if (completed == delayTask)
                        throw new TimeoutException(FormatTimeoutMessage(memberName, filePath, lineNumber));

                    task.GetAwaiter().GetResult();
                }
                finally
                {
                    registration.Dispose();
                    cancel.Cancel();
                }
            }

            return WaitAsync();
        }

        public static Task<T> OrTimeout<T>(this Task<T> task, int ms = 0, int s = 0, int m = 0, int h = 0, int d = 0,
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
                var cancel = new CancellationTokenSource();

                var registration = cancellationToken.RegisterIfCanBeCanceled(cancel);
                try
                {
                    var delayTask = Task.Delay(Debugger.IsAttached ? Timeout.InfiniteTimeSpan : timeout, cancel.Token);

                    var completed = await Task.WhenAny(task, delayTask).ConfigureAwait(false);
                    if (completed == delayTask)
                        throw new TimeoutException(FormatTimeoutMessage(memberName, filePath, lineNumber));

                    return task.GetAwaiter().GetResult();
                }
                finally
                {
                    registration.Dispose();
                    cancel.Cancel();
                }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCompletedSuccessfully(this Task task)
        {
            return task.Status == TaskStatus.RanToCompletion;
        }
    }
}
