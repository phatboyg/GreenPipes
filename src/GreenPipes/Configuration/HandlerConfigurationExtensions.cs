// Copyright 2013-2016 Chris Patterson
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
    using Pipes;


    public static class HandlerConfigurationExtensions
    {
        /// <summary>
        /// Configure a result handler for the pipe
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="handler"></param>
        public static void Handler<TContext, TResult>(this IPipeConfigurator<TContext, TResult> configurator, ContextHandler<TContext, TResult> handler)
            where TContext : class, PipeContext
            where TResult : class
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            configurator.SetHandlerPipe(new DelegateHandlerPipe<TContext, TResult>(handler));
        }
    }
}