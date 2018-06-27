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
    using Mapping;
    using Pipes;


    public static class ResultPipe
    {
        /// <summary>
        /// Returns an empty pipe of the specified context type
        /// </summary>
        /// <typeparam name="T">The context type</typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <returns></returns>
        public static IResultPipe<TContext, T> Default<TContext, T>()
            where TContext : class, PipeContext
        {
            return Cache<TContext, T>.DefaultPipe;
        }


        static class Cache<TContext, T>
            where TContext : class, PipeContext
        {
            internal static readonly IResultPipe<TContext, T> DefaultPipe = new DefaultResultPipe<TContext, T>();
        }
    }
}