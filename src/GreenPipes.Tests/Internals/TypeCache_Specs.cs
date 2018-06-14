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
namespace GreenPipes.Tests.Internals
{
    using System.Text;
    using GreenPipes.Internals.Extensions;
    using NUnit.Framework;


    [TestFixture]
    public class A_type_cache_initialize_from_object
    {
        [Test]
        public void Should_initialize_value_type_array_properties()
        {
            //
            var values = new
            {
                Bytes = Encoding.Default.GetBytes("some string"),
                Ints = new [] {1, 2, 3, 4, 5}
            };

            //
            var notGeneric = TypeCache<INotGeneric>.InitializeFromObject(values);

            //
            Assert.AreEqual(notGeneric.Bytes, values.Bytes);
            Assert.AreEqual(notGeneric.Ints, values.Ints);
        }

        public interface INotGeneric
        {
            byte[] Bytes { get; }

            int[] Ints { get; }
        }
    }
}