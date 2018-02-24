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
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Profile;


    public class ProfileFilter<TContext> :
        IFilter<TContext>
        where TContext : class, PipeContext
    {
        readonly ReportProfileData<TContext> _reportProfileData;
        readonly long _trivialThreshold;
        long _nextTimingId;

        public ProfileFilter(ReportProfileData<TContext> reportProfileData, long trivialThreshold)
        {
            _reportProfileData = reportProfileData;
            _trivialThreshold = trivialThreshold;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("profile");
            scope.Add("trivialThreshold", _trivialThreshold);
        }

        [DebuggerNonUserCode]
        async Task IFilter<TContext>.Send(TContext context, IPipe<TContext> next)
        {
            var timingId = Interlocked.Increment(ref _nextTimingId);

            var instance = new DataRecorder(timingId, context);

            await next.Send(context).ConfigureAwait(false);

            instance.Complete(_reportProfileData, _trivialThreshold);
        }


        struct DataRecorder
        {
            readonly DateTime _startTime;
            readonly long _stopwatchTicks;
            readonly long _timingId;
            readonly TContext _context;

            internal DataRecorder(long timingId, TContext context)
            {
                _timingId = timingId;
                _context = context;
                _startTime = DateTime.UtcNow;
                _stopwatchTicks = Stopwatch.GetTimestamp();
            }

            public void Complete(ReportProfileData<TContext> reportProfileData, long trivialThreshold)
            {
                var completeTicks = Stopwatch.GetTimestamp() - _stopwatchTicks;

                var milliseconds = completeTicks * 1000 / Stopwatch.Frequency;
                if (milliseconds > trivialThreshold)
                    reportProfileData(new Report(_timingId, _context, _startTime, TimeSpan.FromMilliseconds(milliseconds)));
            }


            struct Report :
                ProfileData<TContext>
            {
                public Report(long id, TContext context, DateTime timestamp, TimeSpan elapsed)
                {
                    Id = id;
                    Timestamp = timestamp;
                    Elapsed = elapsed;
                    Context = context;
                }

                public long Id { get; }
                public DateTime Timestamp { get; }
                public TimeSpan Elapsed { get; }

                public TContext Context { get; }
            }
        }
    }
}