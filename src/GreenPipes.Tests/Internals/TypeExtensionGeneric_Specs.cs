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
    using System.Collections.Generic;
    using System.Linq;
    using GreenPipes.Internals.Extensions;
    using NUnit.Framework;


    [TestFixture]
    public class When_getting_the_generic_types_from_an_interface
    {
        [Test]
        public void Should_close_generic_type()
        {
            Assert.IsTrue(typeof(GenericClass).ClosesType(typeof(IGeneric<>)));
        }

        [Test]
        public void Should_not_close_nested_open_generic_base_class()
        {
            Assert.IsFalse(typeof(SuperGenericBaseClass<>).ClosesType(typeof(GenericBaseClass<>)));
        }

        [Test]
        public void Should_not_close_nested_open_generic_interface_in_base_class()
        {
            Assert.IsFalse(typeof(SuperGenericBaseClass<>).ClosesType(typeof(IGeneric<>)));
        }

        [Test]
        public void Should_not_close_open_generic_type()
        {
            Assert.IsFalse(typeof(GenericBaseClass<>).ClosesType(typeof(IGeneric<>)));
        }

        [Test]
        public void Should_not_have_closing_arguments_for_a_class_that_isnt_closed()
        {
            IEnumerable<Type> types = InterfaceExtensions.GetClosingArguments(typeof(SuperGenericBaseClass<>), typeof(IGeneric<>));

            Assert.AreEqual(0, types.Count());
        }

        [Test]
        public void Should_return_the_appropriate_generic_type()
        {
            IEnumerable<Type> types = InterfaceExtensions.GetClosingArguments(typeof(GenericClass), typeof(IGeneric<>));

            Assert.AreEqual(1, types.Count());
            Assert.AreEqual(typeof(int), types.First());
        }

        [Test]
        public void Should_return_the_appropriate_generic_type_for_a_subclass_non_generic()
        {
            IEnumerable<Type> types = InterfaceExtensions.GetClosingArguments(typeof(SubClass), typeof(IGeneric<>));

            Assert.AreEqual(1, types.Count());
            Assert.AreEqual(typeof(int), types.First());
        }

        [Test]
        public void Should_return_the_appropriate_generic_type_with_a_generic_base_class()
        {
            IEnumerable<Type> types = InterfaceExtensions.GetClosingArguments(typeof(NonGenericSubClass), typeof(IGeneric<>));

            Assert.AreEqual(1, types.Count());
            Assert.AreEqual(typeof(int), types.First());
        }

        [Test]
        public void Should_return_the_generic_type_from_a_class()
        {
            IEnumerable<Type> types = InterfaceExtensions.GetClosingArguments(typeof(NonGenericSubClass), typeof(GenericBaseClass<>));

            Assert.AreEqual(1, types.Count());
            Assert.AreEqual(typeof(int), types.First());
        }


        interface IGeneric<T>
        {
        }


        class GenericClass :
            IGeneric<int>
        {
        }


        class SubClass :
            GenericClass
        {
        }


        class SuperGenericBaseClass<T> :
            GenericBaseClass<T>
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