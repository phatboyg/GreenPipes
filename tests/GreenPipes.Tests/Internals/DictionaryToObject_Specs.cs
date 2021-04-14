namespace GreenPipes.Tests.Internals.Mapping
{
    using System;
    using System.Collections.Generic;
    using GreenPipes.Internals.Mapping;
    using GreenPipes.Internals.Reflection;
    using NUnit.Framework;


    [TestFixture]
    public class Converting_a_dictionary_to_an_object
    {
        [Test]
        public void Should_include_a_string()
        {
            Assert.IsNotEmpty(_values.StringValue);
            Assert.AreEqual("Hello", _values.StringValue);
        }

        [Test]
        public void Should_include_nullable_long()
        {
            Assert.IsTrue(_values.LongValue.HasValue);
            Assert.AreEqual(123, _values.LongValue.Value);
        }

        [Test]
        public void Should_include_the_bag_of_dicts()
        {
            Assert.IsNotNull(_values.BagOfDicts);
            Assert.AreEqual(2, _values.BagOfDicts.Count);

            Assert.IsTrue(_values.BagOfDicts.ContainsKey("First"));
            Assert.AreEqual("One", _values.BagOfDicts["First"]);
            Assert.IsTrue(_values.BagOfDicts.ContainsKey("Second"));
            Assert.AreEqual("Two", _values.BagOfDicts["Second"]);
        }

        [Test]
        public void Should_include_the_enum()
        {
            Assert.AreEqual(ValueType.Integer, _values.ValueType);
        }

        [Test]
        public void Should_include_the_enum_as_a_string()
        {
            Assert.AreEqual(ValueType.String, _values.ValueTypeAsString);
        }

        [Test]
        public void Should_include_the_enum_as_an_integer()
        {
            Assert.AreEqual(ValueType.Integer, _values.ValueTypeAsInt);
        }

        [Test]
        public void Should_include_the_integer()
        {
            Assert.AreEqual(27, _values.IntValue);
        }

        [Test]
        public void Should_include_the_list_of_sub_values()
        {
            Assert.IsNotNull(_values.ListOfSubValues);
            Assert.AreEqual(2, _values.ListOfSubValues.Count);

            Assert.AreEqual("A", _values.ListOfSubValues[0].A);
            Assert.AreEqual("B", _values.ListOfSubValues[0].B);
            Assert.AreEqual(new IntentionalTestException().Message, _values.ListOfSubValues[0].Exception.Message);
            Assert.AreEqual("loopback://localhost/1", _values.ListOfSubValues[0].Address.ToString());

            Assert.AreEqual("1", _values.ListOfSubValues[1].A);
            Assert.AreEqual("2", _values.ListOfSubValues[1].B);
        }

        [Test]
        public void Should_include_the_sub_value()
        {
            Assert.IsNotNull(_values.SubValue);

            Assert.AreEqual("A", _values.SubValue.A);
            Assert.AreEqual("B", _values.SubValue.B);
        }

        [Test]
        public void Should_include_the_sub_values()
        {
            Assert.IsNotNull(_values.SubValues);
            Assert.AreEqual(2, _values.SubValues.Length);

            Assert.AreEqual("A", _values.SubValues[0].A);
            Assert.AreEqual("B", _values.SubValues[0].B);

            Assert.AreEqual("1", _values.SubValues[1].A);
            Assert.AreEqual("2", _values.SubValues[1].B);
        }

        IDictionary<string, object> _dictionary;
        Values _values;

        [OneTimeSetUp]
        public void Setup()
        {
            _dictionary = new Dictionary<string, object>
            {
                {"IntValue", 27},
                {"StringValue", "Hello"},
                {"LongValue", (long?)123},
                {"ValueType", ValueType.Integer},
                {"ValueTypeAsInt", 2},
                {"Address", new Uri("loopback://localhost")},
                {"Exception", new IntentionalTestException()},
                {"ValueTypeAsString", "String"},
                {
                    "SubValue", new Dictionary<string, object>
                    {
                        {"A", "A"},
                        {"B", "B"}
                    }
                },
                {"StringValues", new object[] {"A", "B", "C"}},
                {
                    "SubValues", new object[]
                    {
                        new Dictionary<string, object>
                        {
                            {"A", "A"},
                            {"B", "B"},
                            {"Address", new Uri("loopback://localhost/1")}
                        },
                        new Dictionary<string, object>
                        {
                            {"A", "1"},
                            {"B", "2"},
                            {"Address", new Uri("loopback://localhost/2")}
                        }
                    }
                },
                {
                    "ListOfSubValues", new object[]
                    {
                        new Dictionary<string, object>
                        {
                            {"A", "A"},
                            {"B", "B"},
                            {"Address", new Uri("loopback://localhost/1")},
                            {"Exception", new IntentionalTestException()}
                        },
                        new Dictionary<string, object>
                        {
                            {"A", "1"},
                            {"B", "2"},
                            {"Address", new Uri("loopback://localhost/2")},
                            {"Exception", new IntentionalTestException()}
                        }
                    }
                },
                {
                    "BagOfDicts", new object[]
                    {
                        new object[] {"First", "One"},
                        new object[] {"Second", "Two"},
                    }
                }
            };


            IObjectConverterCache converterCache = new DynamicObjectConverterCache(new DynamicImplementationBuilder());

            _values = (Values)converterCache.GetConverter(typeof(Values)).GetObject(_dictionary);
        }


        public interface Values
        {
            int IntValue { get; }
            string StringValue { get; }
            long? LongValue { get; }
            ValueType ValueType { get; }
            ValueType ValueTypeAsInt { get; }
            ValueType ValueTypeAsString { get; }
            SubValue SubValue { get; }
            string[] StringValues { get; }
            SubValue[] SubValues { get; }
            IList<SubValue> ListOfSubValues { get; }
            IDictionary<string, string> BagOfDicts { get; }
            Exception Exception { get; }
            Uri Address { get; }
        }


        public interface SubValue
        {
            string A { get; }
            string B { get; }
            Uri Address { get; }
            Exception Exception { get; }
        }


        public enum ValueType
        {
            Default,
            String,
            Integer
        }
    }
}
