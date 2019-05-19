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
namespace GreenPipes.Specifications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Filters;

    public class LogExtPipeSpecification<T> :
        IPipeSpecification<T>
        where T : class, PipeContext
    {
        readonly Func<T, Task> _logStart;
        readonly Func<T, Task> _logCompleted;
        readonly Func<T, Exception, Task> _logError;

        public LogExtPipeSpecification(Func<T, Task> logStart, Func<T, Task> logCompleted, Func<T, Exception, Task> logError)
        {
            _logStart = logStart;
            _logCompleted = logCompleted;
            _logError = logError;
        }

        void IPipeSpecification<T>.Apply(IPipeBuilder<T> builder)
        {
            builder.AddFilter(new LogExtFilter<T>(_logStart, _logCompleted, _logError));
        }

        IEnumerable<ValidationResult> ISpecification.Validate()
        {
            return Enumerable.Empty<ValidationResult>();
        }
    }
}