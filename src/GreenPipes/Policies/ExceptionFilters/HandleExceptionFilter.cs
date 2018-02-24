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
    using System.Linq;
    using System.Reflection;


    public class HandleExceptionFilter :
        IExceptionFilter
    {
        readonly Type[] _exceptionTypes;

        public HandleExceptionFilter(params Type[] exceptionTypes)
        {
            _exceptionTypes = exceptionTypes;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateScope("selected");
            scope.Set(new
            {
                ExceptionTypes = _exceptionTypes.Select(x => x.Name).ToArray()
            });
        }

        bool IExceptionFilter.Match(Exception exception)
        {
            for (var i = 0; i < _exceptionTypes.Length; i++)
                if (_exceptionTypes[i].GetTypeInfo().IsInstanceOfType(exception))
                    return true;

            return false;
        }
    }
}