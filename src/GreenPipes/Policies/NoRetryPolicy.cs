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
namespace GreenPipes.Policies
{
    using System;
    using System.Threading.Tasks;
    using Util;


    public class NoRetryPolicy :
        RetryPolicy
    {
        void IProbeSite.Probe(ProbeContext context)
        {
            context.Set(new
            {
                Policy = "None"
            });
        }

        public Task<bool> CanRetry(Exception exception, out RetryContext retryContext)
        {
            retryContext = new NoRetryContext(exception);

            return TaskUtil.False;
        }

        public override string ToString()
        {
            return "None";
        }


        class NoRetryContext :
            RetryContext
        {
            public NoRetryContext(Exception exception)
            {
                Exception = exception;
            }

            public Exception Exception { get; }

            public int RetryCount => 0;

            public TimeSpan? Delay => default(TimeSpan?);

            public Task<bool> CanRetry(Exception exception, out RetryContext retryContext)
            {
                retryContext = this;

                return TaskUtil.False;
            }
        }
    }
}