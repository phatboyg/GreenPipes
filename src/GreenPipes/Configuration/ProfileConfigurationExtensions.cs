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
    using Contracts;
    using Filters.Profile;
    using Specifications;


    public static class ProfileConfigurationExtensions
    {
        public static void UseProfile<T>(this IPipeConfigurator<T> configurator, ReportProfileData<T> reportProfileData)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (reportProfileData == null)
                throw new ArgumentNullException(nameof(reportProfileData));

            var specification = new ProfilePipeSpecification<T>(reportProfileData, 0);

            configurator.AddPipeSpecification(specification);
        }

        public static void UseProfile<T>(this IPipeConfigurator<T> configurator, long trivialThreshold, ReportProfileData<T> reportProfileData)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (reportProfileData == null)
                throw new ArgumentNullException(nameof(reportProfileData));

            var specification = new ProfilePipeSpecification<T>(reportProfileData, trivialThreshold);

            configurator.AddPipeSpecification(specification);
        }

        public static void UseConsoleProfile<T>(this IPipeConfigurator<T> configurator, long trivialThreshold = 0)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var pipeBuilderConfigurator = new ProfilePipeSpecification<T>(ConsoleProfileWriter, trivialThreshold);

            configurator.AddPipeSpecification(pipeBuilderConfigurator);
        }

        static void ConsoleProfileWriter<T>(ProfileData<T> data)
            where T : class, PipeContext
        {
            Console.Out.WriteLine($"{data.Id,-6} {data.Timestamp:u} {data.Elapsed:g}");
        }
    }
}