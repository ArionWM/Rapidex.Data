using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Rapidex
{
    public class StringDictionary : DictionaryA<string>
    {
        public StringDictionary() : base()
        {
        }

        public StringDictionary(IDictionary<string, string> dictionary) : base(dictionary)
        {
        }

        public StringDictionary(IEnumerable<KeyValuePair<string, string>> collection) : base(collection)
        {
        }

        public StringDictionary(int capacity) : base(capacity)
        {
        }

        public StringDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
    }
}
