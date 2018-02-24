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
namespace GreenPipes
{
    using System;
    using System.Threading.Tasks;
    using Specifications;


    public static class DelegateConfigurationExtensions
    {
        /// <summary>
        /// Executes a synchronous method on the pipe
        /// </summary>
        /// <typeparam name="TContext">The context type</typeparam>
        /// <param name="configurator">The pipe configurator</param>
        /// <param name="callback">The callback to invoke</param>
        public static void UseExecute<TContext>(this IPipeConfigurator<TContext> configurator, Action<TContext> callback)
            where TContext : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var pipeBuilderConfigurator = new DelegatePipeSpecification<TContext>(callback);

            configurator.AddPipeSpecification(pipeBuilderConfigurator);
        }

        /// <summary>
        /// Executes an asynchronous method on the pipe
        /// </summary>
        /// <typeparam name="TContext">The context type</typeparam>
        /// <param name="configurator">The pipe configurator</param>
        /// <param name="callback">The callback to invoke</param>
        public static void UseExecuteAsync<TContext>(this IPipeConfigurator<TContext> configurator, Func<TContext, Task> callback)
            where TContext : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var pipeBuilderConfigurator = new AsyncDelegatePipeSpecification<TContext>(callback);

            configurator.AddPipeSpecification(pipeBuilderConfigurator);
        }
    }
}