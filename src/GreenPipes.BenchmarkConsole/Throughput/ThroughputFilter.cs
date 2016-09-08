namespace GreenPipes.BenchmarkConsole.Throughput
{
    using System;
    using System.Threading.Tasks;


    public class ThroughputFilter :
        IFilter<TestContext>
    {
        readonly IReportConsumerMetric _report;
        readonly int _faultCount;

        public ThroughputFilter(IReportConsumerMetric report, int faultCount)
        {
            _report = report;
            _faultCount = faultCount;
        }

        public void Probe(ProbeContext context)
        {
        }

        public async Task Send(TestContext context, IPipe<TestContext> next)
        {
            if (_faultCount > 0 && _faultCount < context.Attempts)
            {
                context.Attempts++;

                throw new NotSupportedException("Intentional");
            }

            await _report.Consumed<ThroughputTestContext>(context.CorrelationId).ConfigureAwait(false);

            await next.Send(context).ConfigureAwait(false);
        }
    }
}