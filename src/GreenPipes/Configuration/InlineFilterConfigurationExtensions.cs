// Copyright 2012-2016 Chris Patterson
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
    using Specifications;


    public static class InlineFilterConfigurationExtensions
    {
        /// <summary>
        /// Creates an inline filter using a simple async method
        /// </summary>
        /// <typeparam name="T">The context type</typeparam>
        /// <param name="configurator">The pipe configurator</param>
        /// <param name="inlineFilterMethod">The inline filter delegate</param>
        public static void UseInlineFilter<T>(this IPipeConfigurator<T> configurator, InlineFilterMethod<T> inlineFilterMethod)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var pipeBuilderConfigurator = new InlineFilterPipeSpecification<T>(inlineFilterMethod);

            configurator.AddPipeSpecification(pipeBuilderConfigurator);
        }
    }
}