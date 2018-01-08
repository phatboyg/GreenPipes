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
        readonly string _caption;
        readonly TaskCompletionSource<DateTime> _completed;
        readonly object _lock = new object();
        readonly TaskCompletionSource<DateTime> _ready;
        readonly Lazy<CancellationTokenSource> _stopped;
        readonly Lazy<CancellationTokenSource> _stopping;

        TaskCompletionSource<DateTime> _setCompleted;
        CancellationTokenSource _setCompletedCancel;

        TaskCompletionSource<DateTime> _setReady;
        CancellationTokenSource _setReadyCancel;

        /// <summary>
        /// Creates the Agent
        /// </summary>
        /// <param name="caption">The caption displayed if trace is enabled</param>
        public AgentProvocateur(string caption)
        {
            _caption = caption ?? "Unknown";

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
                    // if a previous readyTask is already completed, no sense in trying
                    if (_setReady.Task.IsCompleted)
                        return;

                    _setReadyCancel.Cancel();

                    _setReady = null;
                    _setReadyCancel = null;
                }

                if (_ready.Task.IsCompleted)
                    return;

                var setReady = _setReady = new TaskCompletionSource<DateTime>();
                setReady.Task.ContinueWith(SetReadyInternal, TaskScheduler.Default);

                var setReadyCancel = _setReadyCancel = new CancellationTokenSource();

                void OnCompleted(Task task)
                {
                    if (setReadyCancel.IsCancellationRequested)
                        return;

                    if (task.IsCanceled)
                        setReady.TrySetCanceled();
                    else if (task.IsFaulted)
                        setReady.TrySetException(task.Exception.InnerExceptions);
                    else
                        setReady.TrySetResult(DateTime.UtcNow);
                }

                readyTask.ContinueWith(OnCompleted, TaskScheduler.Default);
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
                    // if a previous completedTask is already completed, no sense in trying
                    if (_setCompleted.Task.IsCompleted)
                        return;
                    
                    _setCompletedCancel.Cancel();

                    _setCompleted = null;
                    _setCompletedCancel = null;
                }

                if (_completed.Task.IsCompleted)
                    return;

                var setCompleted = _setCompleted = new TaskCompletionSource<DateTime>();
                setCompleted.Task.ContinueWith(SetCompletedInternal, TaskScheduler.Default);

                var setCompletedCancel = _setCompletedCancel = new CancellationTokenSource();

                void OnCompleted(Task task)
                {
                    if (setCompletedCancel.IsCancellationRequested)
                        return;

                    if (task.IsCanceled)
                        setCompleted.TrySetCanceled();
                    else if (task.IsFaulted)
                        setCompleted.TrySetException(task.Exception.InnerExceptions);
                    else
                        setCompleted.TrySetResult(DateTime.UtcNow);
                }

                completedTask.ContinueWith(OnCompleted, TaskScheduler.Default);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Agent({_caption})";
        }

        void SetReadyInternal(Task<DateTime> task)
        {
            if (task.IsCanceled)
                _ready.TrySetCanceled();
            else if (task.IsFaulted)
                _ready.TrySetException(task.Exception.InnerExceptions);
            else
                _ready.TrySetResult(task.Result);
        }

        void SetCompletedInternal(Task<DateTime> task)
        {
            if (task.IsCanceled)
                _completed.TrySetCanceled();
            else if (task.IsFaulted)
                _completed.TrySetException(task.Exception.InnerExceptions);
            else
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