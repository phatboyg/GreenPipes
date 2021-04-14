namespace GreenPipes.Tests.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using GreenPipes.Internals.Mapping;
    using GreenPipes.Internals.Reflection;
    using NUnit.Framework;


    [TestFixture]
    public class DynamicBuilder_Specs
    {
        [Test]
        public void Should_handle_two_builders()
        {
            var builder1 = new DynamicImplementationBuilder();
            var builder2 = new DynamicImplementationBuilder();

            var type1 = builder1.GetImplementationType(typeof(Message));
            var type2 = builder1.GetImplementationType(typeof(Message));

            var obj1 = ToObject<Message>(builder1, new {Name = "Chris", Value = 27});

            Assert.That(obj1.Name, Is.EqualTo("Chris"));
            Assert.That(obj1.Value, Is.EqualTo(27));

            var obj2 = ToObject<Message>(builder2, obj1);

            Assert.That(obj2.Name, Is.EqualTo("Chris"));
            Assert.That(obj2.Value, Is.EqualTo(27));

            Console.WriteLine($"Object 1: {obj1.GetType().Name} :: {obj1.GetType().FullName}");
            Console.WriteLine($"Object 2: {obj2.GetType().Name} :: {obj2.GetType().FullName}");

            Console.WriteLine($"Object 1 Assembly: {obj1.GetType().GetTypeInfo().Assembly.FullName}");
        }


        public interface Message
        {
            string Name { get; }
            int Value { get; }
        }


        T ToObject<T>(DynamicImplementationBuilder builder, object values)
        {
            var factory = new DynamicObjectConverterCache(builder);

            IDictionary<string, object> dictionary = new DictionaryConverterCache().GetConverter(values.GetType()).GetDictionary(values);

            return (T)factory.GetConverter(typeof(T)).GetObject(new DictionaryObjectValueProvider(dictionary));
        }
    }
}
