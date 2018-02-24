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
namespace GreenPipes.Configurators
{
    using System;


    public interface IBindConfigurator<TContext>
        where TContext : class, PipeContext
    {
        /// <summary>
        /// Specifies a pipe context source which is used to create the PipeContext bound to the BindContext.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="configureTarget"></param>
        /// <typeparam name="T"></typeparam>
        void Source<T>(IPipeContextSource<T, TContext> source, Action<IBindConfigurator<TContext, T>> configureTarget)
            where T : class, PipeContext;
    }


    /// <summary>
    /// Configures a binding using the specified pipe context source
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TSource"></typeparam>
    public interface IBindConfigurator<TContext, TSource> :
        IPipeConfigurator<BindContext<TContext, TSource>>
        where TSource : class, PipeContext
        where TContext : class, PipeContext
    {
        /// <summary>
        /// Configure a filter on the context pipe, versus the bound pipe
        /// </summary>
        IPipeConfigurator<TContext> ContextPipe { get; }
    }
}