using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Rapidex
{
    [Serializable]
    public class ObjDictionary : DictionaryA<object>
    {
        public ObjDictionary() : base()
        {
        }

        public ObjDictionary(IDictionary<string, object> dictionary) : base(dictionary)
        {
        }

        public ObjDictionary(IEnumerable<KeyValuePair<string, object>> collection) : base(collection)
        {
        }

        public ObjDictionary(int capacity) : base(capacity)
        {
        }

        public ObjDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }


    }
}
