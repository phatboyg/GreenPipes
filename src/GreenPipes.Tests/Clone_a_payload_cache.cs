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
namespace GreenPipes.Tests
{
    using NUnit.Framework;
    using Payloads;


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
        }
    }
}