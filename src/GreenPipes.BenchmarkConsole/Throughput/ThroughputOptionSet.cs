namespace GreenPipes.BenchmarkConsole.Throughput
{
    public class ThroughputOptionSet :
        OptionSet,
        IThroughputSettings
    {
        public ThroughputOptionSet()
        {
            Add<long>("count:", "The number of messages to send", value => MessageCount = value);
            Add<int>("concurrency:", "The number of concurrent consumers", value => ConcurrencyLimit = value);
            Add<int>("clients:", "The number of sending message clients", value => Clients = value);
            Add<int>("payload:", "The size of the additional payload for the message", value => PayloadSize = value);
            Add<int>("retry:", "The number of retries to allow", value => RetryCount = value);
            Add<int>("fault:", "The number of faults to throw", value => FaultCount = value);

            MessageCount = 1000000;
            ConcurrencyLimit = 16;
            Clients = 10;
        }

        public int PayloadSize { get; set; }
        public long MessageCount { get; set; }
        public int Clients { get; set; }
        public int ConcurrencyLimit { get; set; }
        public int RetryCount { get; set; }
        public int FaultCount {get; set; }
    }
}