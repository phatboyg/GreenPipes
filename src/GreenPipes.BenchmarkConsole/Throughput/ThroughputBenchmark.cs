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
            {
                throw new ArgumentException("The clients must be a factor of message count");
            }

            _payload = _settings.PayloadSize > 0 ? new string('*', _settings.PayloadSize) : null;
        }

        public void Run(CancellationToken cancellationToken = default(CancellationToken))
        {
            _capture = new MessageMetricCapture(_settings.MessageCount);

            var pipe = Pipe.New<TestContext>(x =>
            {
                x.UseFilter(new ThroughputFilter(_capture));
            });
            Console.WriteLine("Running Throughput Benchmark");

            RunBenchmark(pipe).Wait(cancellationToken);

            Console.WriteLine("Message Count: {0}", _settings.MessageCount);
            Console.WriteLine("Clients: {0}", _settings.Clients);
            Console.WriteLine("Payload Length: {0}", _payload?.Length ?? 0);
            Console.WriteLine("Concurrency Limit: {0}", _settings.ConcurrencyLimit);

            Console.WriteLine("Total send duration: {0:g}", _sendDuration);
            Console.WriteLine("Send message rate: {0:F2} (msg/s)",
                _settings.MessageCount * 1000 / _sendDuration.TotalMilliseconds);
            Console.WriteLine("Total consume duration: {0:g}", _consumeDuration);
            Console.WriteLine("Consume message rate: {0:F2} (msg/s)",
                _settings.MessageCount * 1000 / _consumeDuration.TotalMilliseconds);

            var messageMetrics = _capture.GetMessageMetrics();

            Console.WriteLine("Avg Ack Time: {0:F0}ms",
                messageMetrics.Average(x => x.AckLatency) * 1000 / Stopwatch.Frequency);
            Console.WriteLine("Min Ack Time: {0:F0}ms",
                messageMetrics.Min(x => x.AckLatency) * 1000 / Stopwatch.Frequency);
            Console.WriteLine("Max Ack Time: {0:F0}ms",
                messageMetrics.Max(x => x.AckLatency) * 1000 / Stopwatch.Frequency);
            Console.WriteLine("Med Ack Time: {0:F0}ms",
                messageMetrics.Median(x => x.AckLatency) * 1000 / Stopwatch.Frequency);
            Console.WriteLine("95t Ack Time: {0:F0}ms",
                messageMetrics.Percentile(x => x.AckLatency) * 1000 / Stopwatch.Frequency);

            Console.WriteLine("Avg Consume Time: {0:F0}ms",
                messageMetrics.Average(x => x.ConsumeLatency) * 1000 / Stopwatch.Frequency);
            Console.WriteLine("Min Consume Time: {0:F0}ms",
                messageMetrics.Min(x => x.ConsumeLatency) * 1000 / Stopwatch.Frequency);
            Console.WriteLine("Max Consume Time: {0:F0}ms",
                messageMetrics.Max(x => x.ConsumeLatency) * 1000 / Stopwatch.Frequency);
            Console.WriteLine("Med Consume Time: {0:F0}ms",
                messageMetrics.Median(x => x.ConsumeLatency) * 1000 / Stopwatch.Frequency);
            Console.WriteLine("95t Consume Time: {0:F0}ms",
                messageMetrics.Percentile(x => x.ConsumeLatency) * 1000 / Stopwatch.Frequency);

            Console.WriteLine();
            DrawResponseTimeGraph(messageMetrics, x => x.ConsumeLatency);
        }

        void DrawResponseTimeGraph(MessageMetric[] metrics, Func<MessageMetric, long> selector)
        {
            var maxTime = metrics.Max(selector);
            var minTime = metrics.Min(selector);

            const int segments = 10;

            var span = maxTime - minTime;
            var increment = span / segments;

            var histogram = (from x in metrics.Select(selector)
                let key = ((x - minTime) * segments / span)
                where key >= 0 && key < segments
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
            {
                stripes[i] = RunStripe(pipe, _settings.MessageCount / _settings.Clients);
            }

            await Task.WhenAll(stripes);

            _sendDuration = await _capture.SendCompleted;
            _consumeDuration = await _capture.ConsumeCompleted;
        }

        async Task RunStripe(IPipe<TestContext> pipe, long messageCount)
        {
            await Task.Yield();

            for (long i = 0; i < messageCount; i++)
            {
                var messageId = Guid.NewGuid();
                var task = pipe.Send(new ThroughputTestContext(messageId, _payload));

                await _capture.Sent(messageId, task);
            }
        }

//        }
//            configurator.Consumer(() => new MessageThroughputFilter(_capture));
//
//                configurator.UseConcurrencyLimit(_settings.ConcurrencyLimit);
//            if (_settings.ConcurrencyLimit > 0)
//        {

//        void ConfigureReceiveEndpoint(IReceiveEndpointConfigurator configurator)
    }
}