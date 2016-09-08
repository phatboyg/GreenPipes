using GreenPipes.Payloads;
using NUnit.Framework;

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
            Assert.That(p.TryGetPayload(out i), Is.True);

            var p2 = p.CreateScope();

            p.GetOrAddPayload(() => new Item2("bill"));

            Item2 i2;
            Assert.That(p2.TryGetPayload(out i2), Is.False);
            Item i1;
            Assert.That(p2.TryGetPayload(out i1), Is.True);
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