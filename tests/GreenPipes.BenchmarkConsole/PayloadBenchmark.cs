// Copyright 2012-2019 Chris Patterson
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
namespace GreenPipes.BenchmarkConsole
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Jobs;
    using Payloads;


    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MemoryDiagnoser, GcServer(true), GcForce]
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
