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
namespace GreenPipes.BenchmarkConsole.Throughput
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;


    /// <summary>
    /// Benchmark that determines the latency of messages between the time the message is published
    /// to the broker until it is acked by RabbitMQ. And then consumed by the message consumer.
    /// </summary>
    public class ThroughputBenchmark
    {
        readonly string _payload;
        readonly IThroughputSettings _settings;
        MessageMetricCapture _capture;
        TimeSpan _consumeDuration;
        TimeSpan _sendDuration;

        public ThroughputBenchmark(IThroughputSettings settings)
        {
            _settings = settings;

            if (settings.MessageCount / settings.Clients * settings.Clients != settings.MessageCount)
                throw new ArgumentException("The clients must be a factor of message count");

            _payload = _settings.PayloadSize > 0 ? new string('*', _settings.PayloadSize) : null;
        }

        public void Run(CancellationToken cancellationToken = default(CancellationToken))
        {
            _capture = new MessageMetricCapture(_settings.MessageCount);

            IPipe<TestContext> pipe = Pipe.New<TestContext>(x =>
            {
                if (_settings.RetryCount > 0)
                    x.UseRetry(r => r.Immediate(_settings.RetryCount));

                if (_settings.ConcurrencyLimit > 0)
                    x.UseConcurrencyLimit(_settings.ConcurrencyLimit);

                x.UseExecuteAsync(async context =>
                {
                    if (_settings.Yield)
                        await Task.Yield();
                });

                x.UseFilter(new ThroughputFilter(_capture, _settings.FaultCount));
            });


            Console.WriteLine("Running Throughput Benchmark");

            Console.WriteLine(pipe.GetProbeResult().ToJsonString());

            RunBenchmark(pipe).Wait(cancellationToken);

            Console.WriteLine("Message Count: {0}", _settings.MessageCount);
            Console.WriteLine("Clients: {0}", _settings.Clients);
            Console.WriteLine("Payload Length: {0}", _payload?.Length ?? 0);
            Console.WriteLine("Concurrency Limit: {0}", _settings.ConcurrencyLimit);
            Console.WriteLine("Yield: {0}", _settings.Yield);
            Console.WriteLine("Retry: {0}", _settings.RetryCount);

            Console.WriteLine("Total send duration: {0:g}", _sendDuration);
            Console.WriteLine("Send message rate: {0:F2} (msg/s)",
                _settings.MessageCount * 1000 / _sendDuration.TotalMilliseconds);
            Console.WriteLine("Total consume duration: {0:g}", _consumeDuration);
            Console.WriteLine("Consume message rate: {0:F2} (msg/s)",
                _settings.MessageCount * 1000 / _consumeDuration.TotalMilliseconds);

            MessageMetric[] messageMetrics = _capture.GetMessageMetrics();

            Console.WriteLine("Avg Consume Time: {0:F0}us",
                messageMetrics.Average(x => x.ConsumeLatency) * 1000000 / Stopwatch.Frequency);
            Console.WriteLine("Min Consume Time: {0:F0}us",
                messageMetrics.Min(x => x.ConsumeLatency) * 1000000 / Stopwatch.Frequency);
            Console.WriteLine("Max Consume Time: {0:F0}us",
                messageMetrics.Max(x => x.ConsumeLatency) * 1000000 / Stopwatch.Frequency);
            Console.WriteLine("Med Consume Time: {0:F0}us",
                messageMetrics.Median(x => x.ConsumeLatency) * 1000000 / Stopwatch.Frequency);
            Console.WriteLine("95t Consume Time: {0:F0}us",
                messageMetrics.Percentile(x => x.ConsumeLatency) * 1000000 / Stopwatch.Frequency);

            Console.WriteLine();
            DrawResponseTimeGraph(messageMetrics, x => x.ConsumeLatency);
        }

        void DrawResponseTimeGraph(MessageMetric[] metrics, Func<MessageMetric, long> selector)
        {
            var maxTime = metrics.Max(selector);
            var minTime = metrics.Min(selector);

            const int segments = 10;

            var span = Math.Max(1, maxTime - minTime);
            var increment = span / segments;

            var histogram = (from x in metrics.Select(selector)
                let key = (x - minTime) * segments / span
                where (key >= 0) && (key < segments)
                let groupKey = key
                group x by groupKey
                into segment
                orderby segment.Key
                select new {Value = segment.Key, Count = segment.Count()}).ToList();

            var maxCount = histogram.Max(x => x.Count);

            foreach (var item in histogram)
            {
                var barLength = item.Count * 60 / maxCount;
                Console.WriteLine("{0,5}ms {2,-60} ({1,7})", (minTime + increment * item.Value) * 1000 / Stopwatch.Frequency, item.Count,
                    new string('*', barLength));
            }
        }

        async Task RunBenchmark(IPipe<TestContext> pipe)
        {
            await Task.Yield();

            var stripes = new Task[_settings.Clients];

            for (var i = 0; i < _settings.Clients; i++)
                stripes[i] = RunStripe(pipe, _settings.MessageCount / _settings.Clients);

            await Task.WhenAll(stripes).ConfigureAwait(false);

            _sendDuration = await _capture.SendCompleted.ConfigureAwait(false);
            _consumeDuration = await _capture.ConsumeCompleted.ConfigureAwait(false);
        }

        async Task RunStripe(IPipe<TestContext> pipe, long messageCount)
        {
            await Task.Yield();

            for (long i = 0; i < messageCount; i++)
            {
                var messageId = Guid.NewGuid();
                var context = new ThroughputTestContext(messageId, _payload);

                _capture.Sent(messageId);

                await pipe.Send(context).ConfigureAwait(false);
            }
        }
    }
}