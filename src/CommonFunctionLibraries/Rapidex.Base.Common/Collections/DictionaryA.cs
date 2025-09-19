using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Rapidex
{
    public class DictionaryA<T> : Dictionary<string, T> where T : notnull
    {
        public DictionaryA() : base(StringComparer.InvariantCultureIgnoreCase)
        {

        }

        public DictionaryA(IDictionary<string, T> dictionary) : base(dictionary, StringComparer.InvariantCultureIgnoreCase)
        {

        }

        public DictionaryA(IEnumerable<KeyValuePair<string, T>> collection) : base(collection, StringComparer.InvariantCultureIgnoreCase)
        {

        }

        public DictionaryA(int capacity) : base(capacity, StringComparer.InvariantCultureIgnoreCase)
        {

        }

        public DictionaryA(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
