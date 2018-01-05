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
    using System.Threading;
    using System.Threading.Tasks;
    using Agents;
    using Util;


    /// <summary>
    /// An Agent Provocateur that simply exists, out of context
    /// </summary>
    public class AgentProvocateur :
        IAgentProvocateur
    {
        readonly TaskCompletionSource<DateTime> _completed;
        readonly object _lock = new object();
        readonly TaskCompletionSource<DateTime> _ready;
        readonly Lazy<CancellationTokenSource> _stopped;
        readonly Lazy<CancellationTokenSource> _stopping;

        TaskCompletionSource<DateTime> _setCompleted;
        CancellationTokenSource _setCompletedCancel;

        TaskCompletionSource<DateTime> _setReady;
        CancellationTokenSource _setReadyCancel;

        public AgentProvocateur()
        {
            _ready = new TaskCompletionSource<DateTime>();
            _completed = new TaskCompletionSource<DateTime>();

            _stopped = new Lazy<CancellationTokenSource>(() => new CancellationTokenSource());
            _stopping = new Lazy<CancellationTokenSource>(() => new CancellationTokenSource());
        }

        /// <summary>
        /// True if the agent is in the process of stopping or is stopped
        /// </summary>
        protected bool IsStopping => _stopping.IsValueCreated && _stopping.Value.IsCancellationRequested;

        /// <summary>
        /// True if the agent is stopped
        /// </summary>
        protected bool IsStopped => _stopped.IsValueCreated && _stopped.Value.IsCancellationRequested;

        Task IAgent.Ready => _ready.Task;
        Task IAgent.Completed => _completed.Task;
        CancellationToken IAgent.Stopping => _stopping.Value.Token;

        void IAgentProvocateur.SetReady()
        {
            _ready.TrySetResult(DateTime.UtcNow);
        }

        void IAgentProvocateur.SetCompleted()
        {
            _completed.TrySetResult(DateTime.UtcNow);
        }

        /// <inheritdoc />
        public async Task Stop(StopContext context)
        {
            if (_stopping.IsValueCreated)
                _stopping.Value.Cancel();

            await StopAgent(context).ConfigureAwait(false);

            if (_stopped.IsValueCreated)
                _stopped.Value.Cancel();
        }

        /// <summary>
        /// Stops the agent
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual Task StopAgent(StopContext context)
        {
            _completed.TrySetResult(DateTime.UtcNow);

            return TaskUtil.Completed;
        }

        /// <summary>
        /// Set the agent ready for duty
        /// </summary>
        /// <param name="readyTask"></param>
        protected void SetReady(Task readyTask)
        {
            lock (_lock)
            {
                if (_setReady != null)
                {
                    _setReadyCancel.Cancel();

                    _setReady = null;
                    _setReadyCancel = null;
                }

                if (readyTask.Status == TaskStatus.RanToCompletion)
                {
                    _ready.TrySetResult(DateTime.UtcNow);
                    return;
                }

                var setReady = _setReady = new TaskCompletionSource<DateTime>();
                setReady.Task.ContinueWith(SetReadyInternal, TaskScheduler.Default);

                var setReadyCancel = _setReadyCancel = new CancellationTokenSource();

                void OnCompleted(Task task)
                {
                    if (!setReadyCancel.IsCancellationRequested)
                        setReady.TrySetResult(DateTime.UtcNow);
                }

                void OnCanceled(Task task)
                {
                    if (!setReadyCancel.IsCancellationRequested)
                        setReady.TrySetCanceled();
                }

                void OnFaulted(Task task)
                {
                    if (!setReadyCancel.IsCancellationRequested)
                        setReady.TrySetException(task.Exception.InnerExceptions);
                }

                readyTask.ContinueWith(OnCompleted, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
                readyTask.ContinueWith(OnCanceled, CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled, TaskScheduler.Default);
                readyTask.ContinueWith(OnFaulted, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
            }
        }

        /// <summary>
        /// Set the agent Completed for duty
        /// </summary>
        /// <param name="completedTask"></param>
        protected void SetCompleted(Task completedTask)
        {
            lock (_lock)
            {
                if (_setCompleted != null)
                {
                    _setCompletedCancel.Cancel();

                    _setCompleted = null;
                    _setCompletedCancel = null;
                }

                if (completedTask.Status == TaskStatus.RanToCompletion)
                {
                    _completed.TrySetResult(DateTime.UtcNow);
                    return;
                }

                var setCompleted = _setCompleted = new TaskCompletionSource<DateTime>();
                setCompleted.Task.ContinueWith(SetCompletedInternal, TaskScheduler.Default);

                var setCompletedCancel = _setCompletedCancel = new CancellationTokenSource();

                void OnCompleted(Task task)
                {
                    if (!setCompletedCancel.IsCancellationRequested)
                        setCompleted.TrySetResult(DateTime.UtcNow);
                }

                void OnCanceled(Task task)
                {
                    if (!setCompletedCancel.IsCancellationRequested)
                        setCompleted.TrySetCanceled();
                }

                void OnFaulted(Task task)
                {
                    if (!setCompletedCancel.IsCancellationRequested)
                        setCompleted.TrySetException(task.Exception.InnerExceptions);
                }

                completedTask.ContinueWith(OnCompleted, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
                completedTask.ContinueWith(OnCanceled, CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled, TaskScheduler.Default);
                completedTask.ContinueWith(OnFaulted, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
            }
        }

        void SetReadyInternal(Task<DateTime> task)
        {
            _ready.TrySetResult(task.Result);
        }

        void SetCompletedInternal(Task<DateTime> task)
        {
            _completed.TrySetResult(task.Result);
        }

        /// <summary>
        /// Set the agent faulted, making it dead.
        /// </summary>
        /// <param name="task"></param>
        protected void SetFaulted(Task task)
        {
            if (task.IsCanceled)
                _ready.TrySetCanceled();
            else if (task.IsFaulted && task.Exception != null)
                _ready.TrySetException(task.Exception.InnerExceptions);
            else
                _ready.TrySetException(new InvalidOperationException("The context faulted but no exception was present."));

            _completed.TrySetResult(DateTime.UtcNow);
        }
    }
}