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
namespace GreenPipes.Caching
{
    using System;


    public class CacheSettings
    {
        public CacheSettings(int capacity = 10000, TimeSpan? minAge = default, TimeSpan? maxAge = default, CurrentTimeProvider nowProvider = default)
        {
            Capacity = capacity;

            MinAge = minAge ?? TimeSpan.FromMinutes(1);
            MaxAge = maxAge ?? TimeSpan.FromHours(24);

            NowProvider = nowProvider ?? CurrentTime;

            BucketCount = 20;
            TimeSlots = 240;
        }

        /// <summary>
        /// The number of items allowed in the cache. This isn't a hard limit, but the cache will shrink
        /// the cache size to be under the capacity when possible.
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// The maximum length of time an unused item will remain in the cache
        /// </summary>
        public TimeSpan MaxAge { get; set; }

        /// <summary>
        /// The minimum length of time an item will remain in the cache before it is eligible for removal
        /// </summary>
        public TimeSpan MinAge { get; set; }

        /// <summary>
        /// Provides the current time, which is used to manage item lifetime. Replace this to allow unit
        /// tests to move time forward quickly.
        /// </summary>
        public CurrentTimeProvider NowProvider { get; set; }

        /// <summary>
        /// The number of buckets to create per time slot (do NOT change this unless you're very smart)
        /// </summary>
        public int BucketCount { get; set; }

        /// <summary>
        /// The number of time slots per bucket (do NOT change this unless you're very smart)
        /// </summary>
        public int TimeSlots { get; set; }

        static DateTime CurrentTime()
        {
            return DateTime.UtcNow;
        }
    }
}