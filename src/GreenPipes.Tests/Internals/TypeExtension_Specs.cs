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
    using System;
    using GreenPipes.Internals.Extensions;
    using NUnit.Framework;


    [TestFixture]
    public class An_object_that_implements_a_generic_interface
    {
        [Test]
        public void Should_match_a_generic_base_class_implementation_of_the_interface()
        {
            Assert.IsTrue(typeof(NonGenericSubClass).HasInterface<IGeneric<int>>());
        }

        [Test]
        public void Should_match_a_generic_interface()
        {
            Assert.IsTrue(typeof(GenericClass).HasInterface<IGeneric<int>>());
        }

        [Test]
        public void Should_match_a_regular_interface_by_type_argument_on_an_object()
        {
            Assert.IsTrue(typeof(GenericClass).HasInterface(typeof(INotGeneric)));
        }

        [Test]
        public void Should_match_a_regular_interface_on_an_object()
        {
            Assert.IsTrue(typeof(GenericClass).HasInterface<INotGeneric>());
        }

        [Test]
        public void Should_match_a_regular_interface_using_the_generic_argument()
        {
            Assert.IsTrue(typeof(GenericClass).HasInterface<INotGeneric>());
        }

        [Test]
        public void Should_match_a_regular_interface_using_the_generic_argument_on_a_subclass()
        {
            Assert.IsTrue(typeof(GenericSubClass).HasInterface<INotGeneric>());
        }

        [Test]
        public void Should_match_a_regular_interface_using_the_type_argument()
        {
            Assert.IsTrue(typeof(GenericClass).HasInterface(typeof(INotGeneric)));
        }

        [Test]
        public void Should_match_a_regular_interface_using_the_type_argument_on_a_subclass()
        {
            Assert.IsTrue(typeof(GenericSubClass).HasInterface(typeof(INotGeneric)));
        }

        [Test]
        public void Should_match_an_open_generic_interface()
        {
            Assert.IsTrue(typeof(GenericClass).HasInterface(typeof(IGeneric<>)));
        }

        [Test]
        public void Should_match_an_open_generic_interface_in_a_base_class()
        {
            Assert.IsTrue(typeof(NonGenericSubClass).HasInterface(typeof(IGeneric<>)));
        }

        [Test]
        public void Should_not_match_a_regular_interface_that_is_not_implemented()
        {
            Assert.IsFalse(typeof(GenericClass).HasInterface<IDisposable>());
        }


        interface INotGeneric
        {
        }


        interface IGeneric<T>
        {
        }


        class GenericClass :
            IGeneric<int>,
            INotGeneric
        {
        }


        class GenericSubClass :
            GenericClass
        {
        }


        class GenericBaseClass<T> :
            IGeneric<T>
        {
        }


        class NonGenericSubClass :
            GenericBaseClass<int>
        {
        }
    }
}