namespace GreenPipes.Tests.Caching
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GreenPipes.Caching;
    using GreenPipes.Caching.Internals;
    using NUnit.Framework;
    using TestValueObjects;


    [TestFixture]
    public class Adding_nodes_to_a_bucket
    {
        [Test]
        public async Task Should_add_the_first_node()
        {
            var settings = new CacheSettings(1000, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60));
            var manager = new NodeTracker<SimpleValue>(settings);

            Bucket<SimpleValue> bucket = new Bucket<SimpleValue>(manager);

            var valueNode = new BucketNode<SimpleValue>(await SimpleValueFactory.Healthy("Hello"));
            bucket.Push(valueNode);
        }

        [Test]
        public async Task Should_fill_up_the_buckets()
        {
            var settings = new TestCacheSettings(100, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60));

            var cache = new GreenCache<SimpleValue>(settings);

            var index = cache.AddIndex("id", x => x.Id);

            var observer = new NodeAddedCountObserver(100);
            cache.Connect(observer);

            for (int i = 0; i < 100; i++)
            {
                SimpleValue simpleValue = await index.Get($"key{i}", SimpleValueFactory.Healthy);
            }

            await observer.Completed;

            Assert.That(cache.Statistics.Count, Is.EqualTo(100));
        }

        [Test]
        public async Task Should_fill_up_the_buckets_over_time()
        {
            var settings = new TestCacheSettings(100, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(300));

            var cache = new GreenCache<SimpleValue>(settings);

            var index = cache.AddIndex("id", x => x.Id);

            var observer = new NodeAddedCountObserver(100);
            cache.Connect(observer);

            for (int i = 0; i < 100; i++)
            {
                SimpleValue simpleValue = await index.Get($"key{i}", SimpleValueFactory.Healthy);

                settings.CurrentTime += TimeSpan.FromSeconds(1);
            }

            await observer.Completed;

            Assert.That(cache.Statistics.Count, Is.EqualTo(100));
        }


        class NodeAddedCountObserver :
            ICacheValueObserver<SimpleValue>
        {
            readonly TaskCompletionSource<bool> _source;
            int _count;
            readonly int _expected;

            public NodeAddedCountObserver(int expected)
            {
                _expected = expected;
                _source = new TaskCompletionSource<bool>();
            }

            public void ValueAdded(INode<SimpleValue> node, SimpleValue value)
            {
                if (Interlocked.Increment(ref _count) == _expected)
                    _source.TrySetResult(true);
            }

            public void ValueRemoved(INode<SimpleValue> node, SimpleValue value)
            {
            }

            public void CacheCleared()
            {
            }

            public Task<bool> Completed => _source.Task;
        }


        class NodeRemovedCountObserver :
            ICacheValueObserver<SimpleValue>
        {
            readonly TaskCompletionSource<bool> _source;
            int _count;
            readonly int _expected;

            public NodeRemovedCountObserver(int expected)
            {
                _expected = expected;
                _source = new TaskCompletionSource<bool>();
            }

            public void ValueAdded(INode<SimpleValue> node, SimpleValue value)
            {
            }

            public void ValueRemoved(INode<SimpleValue> node, SimpleValue value)
            {
                if (Interlocked.Increment(ref _count) == _expected)
                    _source.TrySetResult(true);
            }

            public void CacheCleared()
            {
            }

            public Task<bool> Completed => _source.Task;
        }


        [Test]
        public async Task Should_fill_up_a_bunch_of_buckets()
        {
            var addedObserver = new NodeAddedCountObserver(100);
            var removedObserver = new NodeRemovedCountObserver(40);

            var settings = new TestCacheSettings(100, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60));

            var cache = new GreenCache<SimpleValue>(settings);

            var index = cache.AddIndex("id", x => x.Id);

            cache.Connect(addedObserver);
            cache.Connect(removedObserver);

            for (int i = 0; i < 100; i++)
            {
                SimpleValue simpleValue = await index.Get($"key{i}", SimpleValueFactory.Healthy);

                settings.CurrentTime += TimeSpan.FromSeconds(1);
            }

            await addedObserver.Completed;
            await removedObserver.Completed;

            Assert.That(cache.Statistics.Count, Is.EqualTo(60));

            var values = cache.GetAll().ToArray();

            Assert.That(values.Length, Is.EqualTo(60));
        }

        [Test]
        public async Task Should_fill_them_even_fuller()
        {
            var settings = new TestCacheSettings(100, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60));

            var cache = new GreenCache<SimpleValue>(settings);

            var index = cache.AddIndex("id", x => x.Id);


            var addedObserver = new NodeAddedCountObserver(200);
            cache.Connect(addedObserver);

            var removedObserver = new NodeRemovedCountObserver(99);
            cache.Connect(removedObserver);

            for (int i = 0; i < 200; i++)
            {
                SimpleValue simpleValue = await index.Get($"key{i}", SimpleValueFactory.Healthy);

                if (i % 2 == 0)
                    settings.CurrentTime += TimeSpan.FromSeconds(1);
            }

            await addedObserver.Completed;
            await removedObserver.Completed;

            Assert.That(cache.Statistics.Count, Is.EqualTo(101));

            var values = cache.GetAll().ToArray();

            Assert.That(values.Length, Is.EqualTo(101));
        }
    }
}