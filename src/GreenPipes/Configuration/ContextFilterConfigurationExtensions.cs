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
    using System;
    using System.Threading.Tasks;
    using Specifications;


    public static class ContextFilterConfigurationExtensions
    {
        /// <summary>
        /// Adds a content filter that uses a delegate to filter the context and only accept messages 
        /// which pass the filter specification.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="filter">A filter method that returns true to accept the message, or false to discard it</param>
        public static void UseContextFilter<T>(this IPipeConfigurator<T> configurator, Func<T, Task<bool>> filter)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            var specification = new ContextFilterPipeSpecification<T>(filter);

            configurator.AddPipeSpecification(specification);
        }
    }
}