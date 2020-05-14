namespace GreenPipes.Internals.Mapping
{
    using System;
    using System.Collections.Generic;


    public class DictionaryObjectValueProvider :
        IObjectValueProvider
    {
        readonly IDictionary<string, object> _dictionary;

        public DictionaryObjectValueProvider(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        public bool TryGetValue(string name, out object value)
        {
            var found = _dictionary.TryGetValue(name, out value);

            if (found)
            {
                if (value is IDictionary<string, object> dict)
                    value = new DictionaryObjectValueProvider(dict);

                if (value is Array arr)
                    value = new ObjectArrayValueProvider(arr);
            }

            return found;
        }

        public bool TryGetValue<T>(string name, out T value)
        {
            object obj;
            if (TryGetValue(name, out obj))
            {
                if (obj is T)
                {
                    value = (T)obj;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}
