namespace GreenPipes.Tests.Caching
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GreenPipes.Caching;
    using GreenPipes.Caching.Internals;
    using NUnit.Framework;
    using TestValueObjects;


    [TestFixture]
    public class Using_an_index_to_add_an_item
    {
        [Test]
        public async Task Should_support_a_simple_addition()
        {
            var index = new GreenCache<SimpleValue>().AddIndex("id", x => x.Id);

            var helloKey = "Hello";

            var value = await index.Get(helloKey, SimpleValueFactory.Healthy);

            Assert.That(value, Is.Not.Null);
            Assert.That(value.Id, Is.EqualTo(helloKey));
            Assert.That(value.Value, Is.EqualTo("The key is Hello"));
        }

        [Test]
        public async Task Should_support_a_simple_addition_and_access()
        {
            var index = new GreenCache<SimpleValue>().AddIndex("id", x => x.Id);

            var helloKey = "Hello";

            var value = await index.Get(helloKey, SimpleValueFactory.Healthy);

            var readValueTask = index.Get(helloKey);

            Assert.That(value, Is.Not.Null);
            Assert.That(value.Id, Is.EqualTo(helloKey));
            Assert.That(value.Value, Is.EqualTo("The key is Hello"));

            var readValue = await readValueTask;

            Assert.That(readValue, Is.Not.Null);
            Assert.That(readValue.Id, Is.EqualTo(helloKey));
            Assert.That(readValue.Value, Is.EqualTo("The key is Hello"));
        }

        [Test]
        public async Task Should_support_access_to_eventual_success()
        {
            var index = new GreenCache<SimpleValue>().AddIndex("id", x => x.Id);

            var helloKey = "Hello";

            var valueTask = index.Get(helloKey, SimpleValueFactory.Faulty);

            var goodValueTask = index.Get(helloKey, SimpleValueFactory.Healthy);

            var readValueTask = index.Get(helloKey);

            Assert.That(async () => await valueTask, Throws.TypeOf<TestException>());

            var value = await goodValueTask;

            Assert.That(value, Is.Not.Null);
            Assert.That(value.Id, Is.EqualTo(helloKey));
            Assert.That(value.Value, Is.EqualTo("The key is Hello"));

            var readValue = await readValueTask;

            Assert.That(readValue, Is.Not.Null);
            Assert.That(readValue.Id, Is.EqualTo(helloKey));
            Assert.That(readValue.Value, Is.EqualTo("The key is Hello"));
        }

        [Test]
        public async Task Should_not_find_a_faulted_value()
        {
            var index = new GreenCache<SimpleValue>().AddIndex("id", x => x.Id);

            var helloKey = "Hello";

            var valueTask = index.Get(helloKey, SimpleValueFactory.Faulty);

            Assert.That(async () => await valueTask, Throws.TypeOf<TestException>());

            Assert.That(async () => await index.Get(helloKey), Throws.TypeOf<KeyNotFoundException>());
        }
    }
}