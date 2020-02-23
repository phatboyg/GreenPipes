// Copyright 2012-2020 Chris Patterson
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
    using Builders;
    using Validation;


    public static class PipeConfiguratorExtensions
    {
        /// <summary>
        /// Validate the pipe configuration, throwing an exception if any failures are encountered.
        /// </summary>
        /// <param name="configurator"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="PipeConfigurationException"></exception>
        public static void ValidatePipeConfiguration<T>(this IBuildPipeConfigurator<T> configurator)
            where T : class, PipeContext
        {
            IPipeConfigurationResult result = new PipeConfigurationResult(configurator.Validate());
            if (result.ContainsFailure)
                throw new PipeConfigurationException(result.GetMessage("The pipe configuration was invalid"));
        }
    }
}
