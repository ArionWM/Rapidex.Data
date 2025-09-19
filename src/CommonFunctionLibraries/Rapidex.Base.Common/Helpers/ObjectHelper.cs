using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Diagnostics.CodeAnalysis;

namespace Rapidex
{
    public static class ObjectHelper
    {
        private static DateTimeOffset zeroTime = new DateTimeOffset(1, 1, 1, 0, 0, 0, TimeSpan.Zero);


        public static bool IsNullOrEmpty([AllowNull] this object? obj)
        {
            if (obj == null)
                return true;

            if (obj is Guid guid)
                return guid == Guid.Empty;

            if (obj is string @string)
            {
                if (string.IsNullOrEmpty(@string))
                    return true;

                if (@string == "(null)")
                    return true;

                if (@string == "null")
                    return true;
            }

            if (obj is DateTime time)
                return time == CommonConstants.NULL_DATE || time == DateTime.MinValue || time == new DateTime(1, 1, 1, 0, 0, 0);

            if (obj is DateTimeOffset time2)
                return time2 == CommonConstants.NULL_DATE || time2 == DateTimeOffset.MinValue || time2 == zeroTime;

            if (obj is IEmptyCheckObject emptyCheckObject)
                return emptyCheckObject.IsEmpty;

            if (obj is Array array && array.Length == 0)
                return true;

            if (obj is IList list && list.Count == 0)
                return true;

            if (obj is IDictionary dict && dict.Count == 0)
                return true;

            if (obj is ICollection coll && coll.Count == 0)
                return true;

            //if(obj is IEnumerable en && en.) Any?

            if (obj is Enum _enum && _enum.IsExists<NullAttribute>())
                return true;

            return false;
        }

        public static bool IsNOTNullOrEmpty([AllowNull, NotNull] this object? obj)
        {
#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.
            return obj != null && !obj.IsNullOrEmpty();
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.
        }

    }
}
