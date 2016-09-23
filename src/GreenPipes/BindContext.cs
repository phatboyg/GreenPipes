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
    /// The binding of a value to the context, which is a fancy form of Tuple
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    public interface BindContext<out TContext, out TTarget> :
        PipeContext
        where TContext : class, PipeContext
        where TTarget : class
    {
        /// <summary>
        /// The original context
        /// </summary>
        TContext Context { get; }

        /// <summary>
        /// The bound target
        /// </summary>
        TTarget Target { get; }
    }
}