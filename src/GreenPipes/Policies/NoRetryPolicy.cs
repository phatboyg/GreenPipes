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
namespace GreenPipes.Policies
{
    using System;


    public class NoRetryPolicy :
        IRetryPolicy
    {
        readonly IExceptionFilter _filter;

        public NoRetryPolicy(IExceptionFilter filter)
        {
            _filter = filter;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            context.Set(new
            {
                Policy = "None"
            });
        }

        RetryPolicyContext<T> IRetryPolicy.CreatePolicyContext<T>(T context)
        {
            return new NoRetryPolicyContext<T>(this, context);
        }

        bool IRetryPolicy.IsHandled(Exception exception)
        {
            return _filter.Match(exception);
        }

        public override string ToString()
        {
            return "None";
        }
    }
}