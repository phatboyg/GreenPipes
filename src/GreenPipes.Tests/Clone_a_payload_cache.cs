using GreenPipes.Payloads;
using NUnit.Framework;
using Shouldly;

namespace GreenPipes.Tests
{
    [TestFixture]
    public class Clone_a_payload_cache
    {
        [Test]
        public void BasicTest()
        {
            IPayloadCache p = new PayloadCache();
            p.GetOrAddPayload(() => new Item("bob"));

            Item i;
            p.TryGetPayload(out i).ShouldBe(true);

            var p2 = p.CreateScope();

            p.GetOrAddPayload(() => new Item2("bill"));

            Item2 i2;
            p2.TryGetPayload(out i2).ShouldBe(false);
            Item i1;
            p2.TryGetPayload(out i1).ShouldBe(true);
        }


        class Item
        {
            public Item(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        class Item2
        {
            public Item2(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        public class ARandomContext
            : BasePipeContext,
            PipeContext
        {
            public ARandomContext() : base(new PayloadCache())
            {
                
            }   
        }
    }
}