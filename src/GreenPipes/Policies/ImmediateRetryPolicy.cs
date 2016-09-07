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


    public class ImmediateRetryPolicy :
        RetryPolicy
    {
        readonly IExceptionFilter _filter;
        readonly int _retryLimit;

        public ImmediateRetryPolicy(IExceptionFilter filter, int retryLimit)
        {
            _filter = filter;
            _retryLimit = retryLimit;
        }

        public int RetryLimit => _retryLimit;

        void IProbeSite.Probe(ProbeContext context)
        {
            context.Set(new
            {
                Policy = "Immediate",
                Limit = _retryLimit
            });

            _filter.Probe(context);
        }

        public Task<bool> CanRetry(Exception exception, out RetryContext retryContext)
        {
            retryContext = new ImmediateRetryContext(this, exception, 0);

            return Matches(exception) ? TaskUtil.True : TaskUtil.False;
        }

        public bool Matches(Exception exception)
        {
            return _filter.Match(exception);
        }
    }
}