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
namespace GreenPipes.Policies.ExceptionFilters
{
    using System;


    public class FilterExceptionFilter<T> :
        IExceptionFilter
        where T : Exception
    {
        readonly Func<T, bool> _filter;

        public FilterExceptionFilter(Func<T, bool> filter)
        {
            _filter = filter;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateScope("filter");
            scope.Set(new
            {
                ExceptionType = typeof(T).Name
            });
        }

        bool IExceptionFilter.Match(Exception exception)
        {
            var currentException = exception;
            while (currentException != null)
            {
                if (exception is T ex)
                    return _filter(ex);

                currentException = currentException.GetBaseException();
            }

            return true;
        }
    }
}