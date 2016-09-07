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


    public class ExponentialRetryContext :
        RetryContext
    {
        readonly ExponentialRetryPolicy _policy;
        readonly int _retryCount;

        public ExponentialRetryContext(ExponentialRetryPolicy policy, Exception exception, int retryCount)
        {
            _policy = policy;
            _retryCount = retryCount;
            Exception = exception;
        }

        public Exception Exception { get; }

        public int RetryCount => _retryCount;

        public TimeSpan? Delay => _policy.Intervals[_retryCount];

        public Task<bool> CanRetry(Exception exception, out RetryContext retryContext)
        {
            retryContext = new ExponentialRetryContext(_policy, Exception, _retryCount + 1);

            var canRetry = _retryCount + 1 < _policy.Intervals.Length && _policy.Matches(exception);

            return canRetry ? TaskUtil.True : TaskUtil.False;
        }
    }
}