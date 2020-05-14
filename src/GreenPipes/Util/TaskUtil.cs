namespace GreenPipes.Util
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;


    public static class TaskUtil
    {
        internal static Task Canceled => Cached<bool>.CanceledTask;
        public static Task Completed => Cached.CompletedTask;
        public static Task<bool> False => Cached.FalseTask;
        public static Task<bool> True => Cached.TrueTask;

        /// <summary>
        /// Returns a completed task with the default value for <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<T> Default<T>()
        {
            return Cached<T>.DefaultValueTask;
        }

        /// <summary>
        /// Returns a faulted task with the specified exception (creating using a <see cref="TaskCompletionSource{T}"/>)
        /// </summary>
        /// <param name="exception"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<T> Faulted<T>(Exception exception)
        {
            TaskCompletionSource<T> source = GetTask<T>();
            source.TrySetException(exception);

            return source.Task;
        }

        /// <summary>
        /// Returns a cancelled task for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<T> Cancelled<T>()
        {
            return Cached<T>.CanceledTask;
        }

        /// <summary>
        /// Creates a new <see cref="TaskCompletionSource{T}"/>, and ensures the TaskCreationOptions.RunContinuationsAsynchronously
        /// flag is specified (if available).
        /// </summary>
        /// <param name="options"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TaskCompletionSource<T> GetTask<T>(TaskCreationOptions options = TaskCreationOptions.None)
        {
            return new TaskCompletionSource<T>(options | TaskCreationOptions.RunContinuationsAsynchronously);
        }

        /// <summary>
        /// Creates a new TaskCompletionSource and ensures the TaskCreationOptions.RunContinuationsAsynchronously
        /// flag is specified (if available).
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TaskCompletionSource<bool> GetTask(TaskCreationOptions options = TaskCreationOptions.None)
        {
            return new TaskCompletionSource<bool>(options | TaskCreationOptions.RunContinuationsAsynchronously);
        }

        /// <summary>
        /// Register a callback on the <paramref name="cancellationToken"/> which completes the resulting task.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="cancelTask"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static CancellationTokenRegistration RegisterTask(this CancellationToken cancellationToken, out Task cancelTask)
        {
            if (!cancellationToken.CanBeCanceled)
                throw new ArgumentException("The cancellationToken must support cancellation", nameof(cancellationToken));

            TaskCompletionSource<bool> source = GetTask();

            cancelTask = source.Task;

            return cancellationToken.Register(SetCompleted, source);
        }

        static void SetCompleted(object obj)
        {
            if (obj is TaskCompletionSource<bool> source)
                source.SetCompleted();
        }

        public static CancellationTokenRegistration RegisterIfCanBeCanceled(this CancellationToken cancellationToken, CancellationTokenSource source)
        {
            if (cancellationToken.CanBeCanceled)
                return cancellationToken.Register(Cancel, source);

            return default;
        }

        static void Cancel(object obj)
        {
            if (obj is CancellationTokenSource source)
                source.Cancel();
        }

        /// <summary>
        /// Sets the source to completed using TrySetResult
        /// </summary>
        /// <param name="source"></param>
        public static void SetCompleted(this TaskCompletionSource<bool> source)
        {
            source.TrySetResult(true);
        }


        static class Cached
        {
            public static readonly Task CompletedTask = Task.FromResult(true);
            public static readonly Task<bool> TrueTask = Task.FromResult(true);
            public static readonly Task<bool> FalseTask = Task.FromResult(false);
        }


        static class Cached<T>
        {
            public static readonly Task<T> DefaultValueTask = Task.FromResult<T>(default);
            public static readonly Task<T> CanceledTask = GetCanceledTask();

            static Task<T> GetCanceledTask()
            {
                TaskCompletionSource<T> source = GetTask<T>();
                source.SetCanceled();
                return source.Task;
            }
        }
    }
}
