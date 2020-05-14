namespace GreenPipes.Internals.Mapping
{
    using System.Collections.Generic;
    using System.Linq;
    using Reflection;


    public class ValueObjectDictionaryDictionaryMapper<T, TKey, TValue> :
        IDictionaryMapper<T>
    {
        readonly IDictionaryConverter _elementConverter;
        readonly ReadOnlyProperty<T> _property;

        public ValueObjectDictionaryDictionaryMapper(ReadOnlyProperty<T> property, IDictionaryConverter elementConverter)
        {
            _property = property;
            _elementConverter = elementConverter;
        }

        public void WritePropertyToDictionary(IDictionary<string, object> dictionary, T obj)
        {
            var value = _property.Get(obj);

            if (!(value is IDictionary<TKey, TValue> values))
                return;

            object[] elementArray = values.Select(element => new object[] {element.Key, _elementConverter.GetDictionary(element.Value)})
                .ToArray<object>();

            dictionary.Add(_property.Property.Name, elementArray);
        }
    }
}
