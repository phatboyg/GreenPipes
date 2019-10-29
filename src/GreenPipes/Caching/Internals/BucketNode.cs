// Copyright 2012-2018 Chris Patterson
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
namespace GreenPipes.Caching.Internals
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Util;


    /// <summary>
    /// A bucket node has been stored in a bucket, and is a fully resolved value.
    /// </summary>
    /// <typeparam name="TValue">The value type</typeparam>
    public class BucketNode<TValue> :
        IBucketNode<TValue>
        where TValue : class
    {
        Bucket<TValue> _bucket;
        IBucketNode<TValue> _next;
        Task<TValue> _value;

        public BucketNode(TValue value)
        {
            _value = Task.FromResult(value);

            if (value is INotifyValueUsed notify)
                notify.Used += Used;
        }

        public Task<TValue> Value
        {
            get
            {
                Used();
                return _value;
            }
        }

        public bool HasValue => true;
        public bool IsValid => _value.Status == TaskStatus.RanToCompletion;

        public Bucket<TValue> Bucket => _bucket;
        public IBucketNode<TValue> Next => _next;

        public Task<TValue> GetValue(IPendingValue<TValue> pendingValue)
        {
            return _value;
        }

        public void SetBucket(Bucket<TValue> bucket, IBucketNode<TValue> next)
        {
            _bucket = bucket;
            _next = next;
        }

        public void AssignToBucket(Bucket<TValue> bucket)
        {
            _bucket = bucket;
        }

        public void Evict()
        {
            _bucket = null;
            _next = null;

            if (_value.Result is INotifyValueUsed notify)
                notify.Used -= Used;

            Interlocked.Exchange(ref _value, Cached.Removed);
        }

        public IBucketNode<TValue> Pop()
        {
            IBucketNode<TValue> next = _next;

            _next = null;

            return next;
        }

        void Used()
        {
            _bucket?.Used(this);
        }


        static class Cached
        {
            internal static readonly Task<TValue> Removed;

            static Cached()
            {
                var source = TaskUtil.GetTask<TValue>();
                source.TrySetException(new InvalidOperationException("The cached value has been removed"));

                Removed = source.Task;
            }
        }
    }
}
