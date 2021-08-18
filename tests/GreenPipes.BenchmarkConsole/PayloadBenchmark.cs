namespace GreenPipes.BenchmarkConsole
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Jobs;
    using Payloads;


    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Net50)]
    [MemoryDiagnoser]
    public class PayloadBenchmark
    {
        [Benchmark(Baseline = true, Description = "No Change")]
        public void NoPayload()
        {
            IPayloadCache cache = new PayloadCache();
        }

        [Benchmark(Description = "One")]
        public void OneGetPayload()
        {
            IPayloadCache cache = new PayloadCache();

            cache.GetOrAddPayload(() => new A());

            cache.TryGetPayload(out A payload);
        }

        [Benchmark(Description = "One (dict)")]
        public void DictionaryOneGetPayload()
        {
            IPayloadCache cache = new ListPayloadCache();

            cache.GetOrAddPayload(() => new A());

            cache.TryGetPayload(out A payload);
        }

        [Benchmark(Description = "Four")]
        public void FourGetPayload()
        {
            IPayloadCache cache = new PayloadCache();

            cache.GetOrAddPayload(() => new A());
            cache.GetOrAddPayload(() => new B());
            cache.GetOrAddPayload(() => new C());
            cache.GetOrAddPayload(() => new D());

            cache.TryGetPayload(out A payload);
        }

        [Benchmark(Description = "Four (dict)")]
        public void DictionaryFourGetPayload()
        {
            IPayloadCache cache = new ListPayloadCache();

            cache.GetOrAddPayload(() => new A());
            cache.GetOrAddPayload(() => new B());
            cache.GetOrAddPayload(() => new C());
            cache.GetOrAddPayload(() => new D());

            cache.TryGetPayload(out A payload);
        }

        [Benchmark(Description = "AddUpdate")]
        public void AddUpdate()
        {
            IPayloadCache cache = new PayloadCache();

            cache.GetOrAddPayload(() => new A());
            cache.GetOrAddPayload(() => new B());
            cache.GetOrAddPayload(() => new C());
            cache.GetOrAddPayload(() => new D());

            cache.AddOrUpdatePayload(() => new A(), x => new A());
            cache.AddOrUpdatePayload(() => new C(), x => new C());
            cache.AddOrUpdatePayload(() => new B(), x => new B());
            cache.TryGetPayload(out A payload);
        }

        [Benchmark(Description = "AddUpdate (dict)")]
        public void DictionaryAddUpdate()
        {
            IPayloadCache cache = new ListPayloadCache();

            cache.GetOrAddPayload(() => new A());
            cache.GetOrAddPayload(() => new B());
            cache.GetOrAddPayload(() => new C());
            cache.GetOrAddPayload(() => new D());

            cache.AddOrUpdatePayload(() => new A(), x => new A());
            cache.AddOrUpdatePayload(() => new C(), x => new C());
            cache.AddOrUpdatePayload(() => new B(), x => new B());
            cache.TryGetPayload(out A payload);
        }

        [Benchmark(Description = "FourAll")]
        public void FourGetAllPayload()
        {
            IPayloadCache cache = new PayloadCache();

            cache.GetOrAddPayload(() => new A());
            cache.GetOrAddPayload(() => new B());
            cache.GetOrAddPayload(() => new C());
            cache.GetOrAddPayload(() => new D());

            cache.TryGetPayload(out A payloadA);
            cache.TryGetPayload(out B payloadB);
            cache.TryGetPayload(out C payloadC);
            cache.TryGetPayload(out D payloadD);
        }

        [Benchmark(Description = "FourAll (dict)")]
        public void DictionaryFourGetAllPayload()
        {
            IPayloadCache cache = new ListPayloadCache();

            cache.GetOrAddPayload(() => new A());
            cache.GetOrAddPayload(() => new B());
            cache.GetOrAddPayload(() => new C());
            cache.GetOrAddPayload(() => new D());

            cache.TryGetPayload(out A payloadA);
            cache.TryGetPayload(out B payloadB);
            cache.TryGetPayload(out C payloadC);
            cache.TryGetPayload(out D payloadD);
        }


        class A
        {
        }


        class B
        {
        }


        class C
        {
        }


        class D
        {
        }
    }
}
