namespace GreenPipes.Internals.Mapping
{
    using System;
    using System.Collections.Generic;


    public class ObjectArrayValueProvider :
        IArrayValueProvider
    {
        readonly Array _values;

        public ObjectArrayValueProvider(Array values)
        {
            _values = values;
        }

        public bool TryGetValue(int index, out object value)
        {
            if (index < 0 || index >= _values.Length)
            {
                value = null;
                return false;
            }

            value = _values.GetValue(index);
            if (value is IDictionary<string, object> dict)
                value = new DictionaryObjectValueProvider(dict);
            else if (value is Array arr)
                value = new ObjectArrayValueProvider(arr);

            return true;
        }

        public bool TryGetValue<T>(int index, out T value)
        {
            object obj;
            if (TryGetValue(index, out obj))
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
