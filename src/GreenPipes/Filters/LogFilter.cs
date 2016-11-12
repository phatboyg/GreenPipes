// Copyright 2012-2016 Chris Patterson
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
    using System.IO;
    using System.Threading.Tasks;
    using Log;


    public class LogFilter<TContext> :
        IFilter<TContext>
        where TContext : class, PipeContext
    {
        readonly LogFormatter<TContext> _formatter;
        readonly TextWriter _writer;

        public LogFilter(TextWriter writer, LogFormatter<TContext> formatter)
        {
            _writer = writer;
            _formatter = formatter;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            context.CreateFilterScope("log");
        }

        [DebuggerNonUserCode]
        async Task IFilter<TContext>.Send(TContext context, IPipe<TContext> next)
        {
            var startTime = DateTime.UtcNow;
            var timer = Stopwatch.StartNew();

            await next.Send(context).ConfigureAwait(false);

            timer.Stop();

            var logContext = new LogContext<TContext>(context, startTime, timer.Elapsed);

            var text = await _formatter(logContext).ConfigureAwait(false);

            await _writer.WriteLineAsync(text).ConfigureAwait(false);
        }
    }
}