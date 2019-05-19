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
namespace GreenPipes.Filters
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class LogExtFilter<TContext> :
        IFilter<TContext>
        where TContext : class, PipeContext
    {
        readonly Func<TContext, Task> _logStart;
        readonly Func<TContext, Task> _logCompleted;
        readonly Func<TContext, Exception, Task> _logError;

        public LogExtFilter(Func<TContext, Task> logStart, Func<TContext, Task> logCompleted, Func<TContext, Exception, Task> logError)
        {
            _logStart = logStart;
            _logCompleted = logCompleted;
            _logError = logError;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            context.CreateFilterScope("log");
        }

        [DebuggerNonUserCode]
        async Task IFilter<TContext>.Send(TContext context, IPipe<TContext> next)
        {
            if (_logStart != null)
                await _logStart(context);

            try
            {
                await next.Send(context).ConfigureAwait(false);

                if (_logCompleted != null)
                    await _logCompleted(context);
            }
            catch (Exception ex)
            {
                if (_logError != null)
                    await _logError(context, ex);

                throw;
            }
        }
    }
}