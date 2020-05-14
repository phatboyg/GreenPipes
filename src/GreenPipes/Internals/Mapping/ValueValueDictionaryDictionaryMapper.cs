namespace GreenPipes.Internals.Mapping
{
    using System.Collections.Generic;
    using System.Linq;
    using Reflection;


    public class ValueValueDictionaryDictionaryMapper<T, TKey, TValue> :
        IDictionaryMapper<T>
    {
        readonly ReadOnlyProperty<T> _property;

        public ValueValueDictionaryDictionaryMapper(ReadOnlyProperty<T> property)
        {
            _property = property;
        }

        public void WritePropertyToDictionary(IDictionary<string, object> dictionary, T obj)
        {
            var value = _property.Get(obj);

            if (!(value is IDictionary<TKey, TValue> values))
                return;

            object[] elementArray = values.Select(element => new object[] {element.Key, element.Value})
                .ToArray<object>();

            dictionary.Add(_property.Property.Name, elementArray);
        }
    }
}
