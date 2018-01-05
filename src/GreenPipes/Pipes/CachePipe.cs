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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Agents;
    using Util;


    /// <summary>
    /// A pipe that caches an instance of T, which is then used to invoke the specified pipe
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CachePipe<T> :
        ICachePipe<T>
        where T : class, PipeContext
    {
        public Task Send(IPipe<T> pipe, CancellationToken cancellationToken = default(CancellationToken))
        {
        }
    }


    public interface IPipeContextFactory<T>
        where T : class, PipeContext
    {
        Task<T> Create(CancellationToken cancellationToken = default(CancellationToken));
    }


    /// <summary>
    /// Factory used to create a new agent, which is a subordinate of this agent
    /// </summary>
    /// <param name="agent"></param>
    /// <typeparam name="T"></typeparam>
    public delegate IContextAgent<T> ContextAgentFactory<T>(IContextAgent<T> agent)
        where T : class, PipeContext;


    public interface IContextAgentFactory<TContext>
        where TContext : class, PipeContext
    {
        IContextAgentProvocateur<TContext> CreateProvocateur(IContextAgent<TContext> agent);

        /// <summary>
        /// Create an agent subordinate to the specified agent
        /// </summary>
        /// <param name="agent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IContextAgentProvocateur<T> CreateAgent<T>(IContextAgent<TContext> agent)
            where T : class, PipeContext;
    }


    public class ContextAgent<TContext> :
        IContextAgent<TContext>
        where TContext : class, PipeContext
    {
        readonly IContextAgentFactory<TContext> _agentFactory;
        readonly Dictionary<long, IContextAgentProvocateur> _provocateurs;
        readonly CancellationTokenSource _stopping;
        readonly CancellationTokenSource _stopped;

        long _nextId;

        public ContextAgent(IContextAgentFactory<TContext> agentFactory)
        {
            _agentFactory = agentFactory;
            _provocateurs = new Dictionary<long, IContextAgentProvocateur>();
            _stopped = new CancellationTokenSource();
            _stopping = new CancellationTokenSource();
        }

        Task IContextAgentProvocateur.Ready
        {
            get
            {
                lock (_provocateurs)
                    return Task.WhenAll(_provocateurs.Values.Select(x => x.Ready).ToArray());
            }
        }

        Task IContextAgentProvocateur.Completed
        {
            get
            {
                lock (_provocateurs)
                    return Task.WhenAll(_provocateurs.Values.Select(x => x.Completed).ToArray());
            }
        }

        IContextAgentProvocateur<TContext> IContextAgent<TContext>.CreateProvocateur()
        {
            if (_stopping.IsCancellationRequested)
                throw new OperationCanceledException("The agent is stopped or has been stopped, no additional provocateurs can be created.");

            var provocateur = _agentFactory.CreateProvocateur(this);

            AddProvocateur(provocateur);

            return provocateur;
        }

        IContextAgentProvocateur<T> IContextAgent<TContext>.CreateAgent<T>()
        {
            if (_stopping.IsCancellationRequested)
                throw new OperationCanceledException("The agent is stopped or has been stopped, no additional provocateurs can be created.");

            var agent = _agentFactory.CreateAgent<T>(this);

            AddProvocateur(agent);

            return agent;
        }

        void AddProvocateur(IContextAgentProvocateur provocateur)
        {
            var id = Interlocked.Increment(ref _nextId);

            lock (_provocateurs)
                _provocateurs.Add(id, provocateur);

            void RemoveProvocateur(Task task)
            {
                Remove(id);
            }

            provocateur.Completed.ContinueWith(RemoveProvocateur, TaskScheduler.Default);
            provocateur.Ready.ContinueWith(RemoveProvocateur, CancellationToken.None, TaskContinuationOptions.NotOnRanToCompletion, TaskScheduler.Default);
        }

        void Remove(long id)
        {
            lock (_provocateurs)
                _provocateurs.Remove(id);
        }

        async Task IContextAgentProvocateur.Stop(StopContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            _stopping.Cancel();

            Task[] provocateurs;
            lock (_provocateurs)
                provocateurs = _provocateurs.Values.Select(x => x.Stop(context, cancellationToken)).ToArray();

            await Task.WhenAll(provocateurs).ConfigureAwait(false);

            _stopped.Cancel();
        }
    }


    /// <summary>
    /// A handle to a PipeContext instance (of type <typeparam name="T">T</typeparam>), which can be discarded
    /// once it is no longer needed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface PipeContextHandle<T>
        where T : class, PipeContext
    {
        /// <summary>
        /// The PipeContext
        /// </summary>
        Task<T> Context { get; }

        /// <summary>
        /// Disconnect/discard the context
        /// </summary>
        /// <returns></returns>
        Task Disconnect();
    }


    /// <summary>
    /// Controls the cached PipeContext instance
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICachedPipeContext<in T>
    {
        /// <summary>
        /// Called when the PipeContext has been created and is available for use.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task Created(T context);

        /// <summary>
        /// Called when the PipeContext creation failed
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        Task CreateFailed(Exception exception);

        /// <summary>
        /// Called when the successfully created PipeContext becomes faulted, indicating that it
        /// should no longer be used.
        /// </summary>
        /// <param name="exception">The exception which occurred</param>
        /// <returns></returns>
        Task Faulted(Exception exception);
    }


    public class CachedPipeContextHandle<T> :
        PipeContextHandle<T>,
        ICachedPipeContext<T>
        where T : class, PipeContext
    {
        readonly TaskCompletionSource<T> _context;

        public CachedPipeContextHandle()
        {
            _context = new TaskCompletionSource<T>();
        }

        Task<T> PipeContextHandle<T>.Context => _context.Task;

        Task PipeContextHandle<T>.Disconnect()
        {
            throw new System.NotImplementedException();
        }

        Task ICachedPipeContext<T>.Created(T context)
        {
            _context.SetResult(context);

            return TaskUtil.Completed;
        }

        Task ICachedPipeContext<T>.CreateFailed(Exception exception)
        {
            _context.SetException(exception);

            return TaskUtil.Completed;
        }

        Task ICachedPipeContext<T>.Faulted(Exception exception)
        {
        }
    }


    /// <summary>
    /// A context agent supports the creation of provocateurs
    /// </summary>
    public interface IContextAgent<TContext> :
        IContextAgentProvocateur
        where TContext : class, PipeContext
    {
        /// <summary>
        /// Signaled when the context is ready, but not necessarily the provocateurs
        /// </summary>
        Task<TContext> Context { get; }

        /// <summary>
        /// Creates a provocateur of the agent, which is owned and controlled by the agent
        /// </summary>
        /// <returns></returns>
        IContextAgentProvocateur<TContext> CreateProvocateur();

        /// <summary>
        /// Creates a provocateur of the agent, which is owned and controlled by the agent
        /// </summary>
        /// <returns></returns>
        IContextAgentProvocateur<T> CreateAgent<T>()
            where T : class, PipeContext;
    }


    public interface IContextAgentProvocateur<TContext> :
        IContextAgentProvocateur
        where TContext : class, PipeContext
    {
    }


    public interface IContextAgentProvocateur
    {
        /// <summary>
        /// The Task used to signal the agent is ready/faulted/cancelled
        /// </summary>
        Task Ready { get; }

        /// <summary>
        /// Completed when the agent is finished, no longer active
        /// </summary>
        Task Completed { get; }

        /// <summary>
        /// Stop the agent provocateur, signaling the agent is going to go away. This method can return immediately
        /// as the <see cref="Completed"/> property is used to ensure it's done.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Stop(StopContext context, CancellationToken cancellationToken = default(CancellationToken));
    }


    class ModelScope
    {
        readonly ITaskScope _taskScope;

        public ModelScope(ITaskScope supervisor)
        {
            _modelContext = new TaskCompletionSource<RabbitMqModelContext>();

            _taskScope = supervisor.CreateScope("ModelScope", CloseContext);
        }

        public bool IsShuttingDown => _taskScope.StoppingToken.IsCancellationRequested;

        public void Created(RabbitMqModelContext connectionContext)
        {
            _modelContext.TrySetResult(connectionContext);

            _taskScope.SetReady();
        }

        public void CreateFaulted(Exception exception)
        {
            _modelContext.TrySetException(exception);

            _taskScope.SetNotReady(exception);

            _taskScope.Stop(new StopEventArgs($"Model faulted: {exception.Message}"));
        }

        public async Task<SharedModelContext> Attach(CancellationToken cancellationToken)
        {
            var modelContext = await _modelContext.Task.ConfigureAwait(false);

            return new SharedModelContext(modelContext, cancellationToken, _taskScope);
        }

        public void Shutdown(string reason)
        {
            _taskScope.Stop(new StopEventArgs(reason));
        }

        async Task CloseContext()
        {
            if (_modelContext.Task.Status == TaskStatus.RanToCompletion)
            {
                try
                {
                    var modelContext = await _modelContext.Task.ConfigureAwait(false);

                    if (_log.IsDebugEnabled)
                        _log.DebugFormat("Disposing model: {0}", ((ModelContext)modelContext).ConnectionContext.HostSettings.ToDebugString());

                    modelContext.Dispose();
                }
                catch (Exception exception)
                {
                    if (_log.IsWarnEnabled)
                        _log.Warn("The model failed to be disposed", exception);
                }
            }
        }
    }


    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICachePipe<out T>
        where T : class, PipeContext
    {
        /// <summary>
        /// Send the T context to the specified pipe, supporting cancellation as requested
        /// </summary>
        /// <param name="pipe">The target pipe</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Send(IPipe<T> pipe, CancellationToken cancellationToken = default(CancellationToken));
    }
}