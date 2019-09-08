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


    public interface IBindConfigurator<TLeft>
        where TLeft : class, PipeContext
    {
        /// <summary>
        /// Specifies a pipe context source which is used to create the PipeContext bound to the BindContext.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="configureTarget"></param>
        /// <typeparam name="T"></typeparam>
        void Source<T>(IPipeContextSource<T, TLeft> source, Action<IBindConfigurator<TLeft, T>> configureTarget)
            where T : class, PipeContext;
    }


    /// <summary>
    /// Configures a binding using the specified pipe context source
    /// </summary>
    /// <typeparam name="TLeft"></typeparam>
    /// <typeparam name="TRight"></typeparam>
    public interface IBindConfigurator<TLeft, TRight> :
        IPipeConfigurator<BindContext<TLeft, TRight>>
        where TRight : class, PipeContext
        where TLeft : class, PipeContext
    {
        /// <summary>
        /// Configure a filter on the context pipe, versus the bound pipe
        /// </summary>
        IPipeConfigurator<TLeft> ContextPipe { get; }
    }
}