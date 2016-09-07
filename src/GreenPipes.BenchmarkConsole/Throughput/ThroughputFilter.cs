namespace GreenPipes.BenchmarkConsole.Throughput
{
    using System.Threading.Tasks;


    public class ThroughputFilter :
        IFilter<TestContext>
    {
        readonly IReportConsumerMetric _report;

        public ThroughputFilter(IReportConsumerMetric report)
        {
            _report = report;
        }

        public void Probe(ProbeContext context)
        {
        }

        public async Task Send(TestContext context, IPipe<TestContext> next)
        {
            await _report.Consumed<ThroughputTestContext>(context.CorrelationId).ConfigureAwait(false);

            await next.Send(context).ConfigureAwait(false);
        }
    }
}