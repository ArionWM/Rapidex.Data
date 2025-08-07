using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Rapidex.Data
{
    public static class DataTableExtensions
    {
        public static T To<T>(this DataRowView row, string columnName, bool throwExceptionIfNotExist = false)
        {
            if (!row.DataView.Table.Columns.Contains(columnName))
            {
                if (throwExceptionIfNotExist)
                    throw new ArgumentNullException(columnName);

                return default(T);
            }

            object objValue = row[columnName];
            if (Convert.IsDBNull(objValue))
            {
                if (throwExceptionIfNotExist)
                    throw new ArgumentNullException(columnName);

                return default(T);
            }

            T value = objValue.As<T>();
            return value;
        }

        public static T To<T>(this DataRow row, string columnName, bool throwExceptionIfNotExist = false)
        {
            if (!row.Table.Columns.Contains(columnName))
            {
                if (throwExceptionIfNotExist)
                    throw new ArgumentNullException(columnName);

                return default(T);
            }

            object objValue = row[columnName];
            if (Convert.IsDBNull(objValue))
            {
                if (throwExceptionIfNotExist)
                    throw new ArgumentNullException(columnName);

                return default(T);
            }

            if (objValue is string strVal)
                objValue = strVal?.Trim();

            T value = objValue.As<T>();
            return value;
        }

        public static T To<T>(this DataRow row, int columnIndex, bool throwExceptionIfNotExist = false)
        {

            object objValue = row[columnIndex];
            if (Convert.IsDBNull(objValue))
            {
                if (throwExceptionIfNotExist)
                    throw new ArgumentNullException($"index: {columnIndex}");

                return default(T);
            }

            T value = objValue.As<T>();
            return value;
        }

        public static IEnumerable<DataRow> AsEnumerable(this DataRowCollection source)
        {
            return source.Cast<DataRow>();
        }

    }
}
