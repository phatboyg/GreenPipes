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
    using Configurators;
    using Filters;
    using Specifications;


    public static class RescueConfigurationExtensions
    {
        /// <summary>
        /// Rescue exceptions via the alternate pipe
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TRescue"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="rescuePipe"></param>
        /// <param name="rescueContextFactory">Factory method to convert the pipe context to the rescue pipe context</param>
        /// <param name="configure"></param>
        public static void UseRescue<T, TRescue>(this IPipeConfigurator<T> configurator, IPipe<TRescue> rescuePipe,
            RescueContextFactory<T, TRescue> rescueContextFactory, Action<IRescueConfigurator> configure = null)
            where T : class, PipeContext
            where TRescue : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var specification = new RescuePipeSpecification<T, TRescue>(rescuePipe, rescueContextFactory);

            configure?.Invoke(specification);

            configurator.AddPipeSpecification(specification);
        }
    }
}