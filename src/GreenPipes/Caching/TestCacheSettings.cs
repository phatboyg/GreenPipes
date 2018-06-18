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


    public class TestCacheSettings :
        CacheSettings
    {
        readonly TestTimeProvider _provider;

        public TestCacheSettings(int capacity = 10000, TimeSpan? minAge = default, TimeSpan? maxAge = default)
            : this(capacity, minAge, maxAge, new TestTimeProvider())
        {
        }

        TestCacheSettings(int capacity, TimeSpan? minAge, TimeSpan? maxAge, TestTimeProvider provider)
            : base(capacity, minAge, maxAge, provider.Now)
        {
            _provider = provider;
        }

        public DateTime CurrentTime
        {
            get => _provider.Current;
            set => _provider.Current = value;
        }


        class TestTimeProvider
        {
            public TestTimeProvider()
            {
                Current = DateTime.UtcNow;
            }

            public DateTime Current;

            public DateTime Now()
            {
                return Current;
            }
        }
    }
}