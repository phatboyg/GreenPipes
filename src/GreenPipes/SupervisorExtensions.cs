namespace GreenPipes
{
    using System;
    using System.Threading.Tasks;
    using Agents;


    public static class SupervisorExtensions
    {
        /// <summary>
        /// Adds a context to the supervisor as an agent, which can be stopped by the supervisor.
        /// </summary>
        /// <param name="supervisor">The supervisor</param>
        /// <param name="context">The context</param>
        /// <typeparam name="T">The context type</typeparam>
        /// <returns>A context handle</returns>
        public static IPipeContextAgent<T> AddContext<T>(this ISupervisor supervisor, T context)
            where T : class, PipeContext
        {
            if (supervisor == null)
                throw new ArgumentNullException(nameof(supervisor));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            IPipeContextAgent<T> contextAgent = new PipeContextAgent<T>(context);

            supervisor.Add(contextAgent);

            return contextAgent;
        }

        /// <summary>
        /// Adds a context to the supervisor as an agent, which can be stopped by the supervisor.
        /// </summary>
        /// <param name="supervisor">The supervisor</param>
        /// <param name="context">The context</param>
        /// <typeparam name="T">The context type</typeparam>
        /// <returns>A context handle</returns>
        public static IPipeContextAgent<T> AddContext<T>(this ISupervisor supervisor, Task<T> context)
            where T : class, PipeContext
        {
            if (supervisor == null)
                throw new ArgumentNullException(nameof(supervisor));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            IPipeContextAgent<T> contextAgent = new PipeContextAgent<T>(context);

            supervisor.Add(contextAgent);

            return contextAgent;
        }

        /// <summary>
        /// Adds a context to the supervisor as an agent, which can be stopped by the supervisor.
        /// </summary>
        /// <param name="supervisor">The supervisor</param>
        /// <param name="contextHandle">The actual context handle</param>
        /// <param name="context">The active context</param>
        /// <typeparam name="T">The context type</typeparam>
        /// <returns>A context handle</returns>
        public static IActivePipeContextAgent<T> AddActiveContext<T>(this ISupervisor supervisor, PipeContextHandle<T> contextHandle, Task<T> context)
            where T : class, PipeContext
        {
            if (supervisor == null)
                throw new ArgumentNullException(nameof(supervisor));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var activeContext = new ActivePipeContext<T>(contextHandle, context);

            var contextAgent = new ActivePipeContextAgent<T>(activeContext);

            supervisor.Add(contextAgent);

            return contextAgent;
        }

        /// <summary>
        /// Adds a context to the supervisor as an agent, which can be stopped by the supervisor.
        /// </summary>
        /// <param name="supervisor">The supervisor</param>
        /// <param name="contextHandle">The actual context handle</param>
        /// <param name="context">The active context</param>
        /// <typeparam name="T">The context type</typeparam>
        /// <returns>A context handle</returns>
        public static IActivePipeContextAgent<T> AddActiveContext<T>(this ISupervisor supervisor, PipeContextHandle<T> contextHandle, T context)
            where T : class, PipeContext
        {
            if (supervisor == null)
                throw new ArgumentNullException(nameof(supervisor));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var activeContext = new ActivePipeContext<T>(contextHandle, context);

            var contextAgent = new ActivePipeContextAgent<T>(activeContext);

            supervisor.Add(contextAgent);

            return contextAgent;
        }

        /// <summary>
        /// Adds a context to the supervisor as an agent, which can be stopped by the supervisor.
        /// </summary>
        /// <param name="supervisor">The supervisor</param>
        /// <typeparam name="T">The context type</typeparam>
        /// <returns>A context handle</returns>
        public static IAsyncPipeContextAgent<T> AddAsyncContext<T>(this ISupervisor supervisor)
            where T : class, PipeContext
        {
            if (supervisor == null)
                throw new ArgumentNullException(nameof(supervisor));

            IAsyncPipeContextAgent<T> contextAgent = new AsyncPipeContextAgent<T>();

            supervisor.Add(contextAgent);

            return contextAgent;
        }
    }
}
