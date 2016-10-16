// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    /// <summary>
    /// The intent is to connect a pipe of a specific type to a pipe of a different type,
    /// for which there is a provider that knows how to convert the input type to the output type.
    /// </summary>
    public interface IPipeConnector
    {
        /// <summary>
        /// Connect a pipe of the specified type to the DispatchFilter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pipe"></param>
        /// <returns></returns>
        ConnectHandle ConnectPipe<T>(IPipe<T> pipe)
            where T : class, PipeContext;
    }


    /// <summary>
    /// Connect a pipe of the same type as the target pipe
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public interface IPipeConnector<out TContext>
        where TContext : class, PipeContext
    {
        ConnectHandle ConnectPipe(IPipe<TContext> pipe);
    }


    /// <summary>
    /// Supports connecting a pipe using a key, which is a method of dispatching to different pipes
    /// based on context.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IKeyPipeConnector<in TKey>
    {
        /// <summary>
        /// Connect a pipe to the filter using the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pipe"></param>
        /// <returns></returns>
        ConnectHandle ConnectPipe<T>(TKey key, IPipe<T> pipe)
            where T : class, PipeContext;
    }
}