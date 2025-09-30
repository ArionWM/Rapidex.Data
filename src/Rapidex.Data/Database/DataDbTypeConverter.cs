using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;

namespace Rapidex.Data
{
    internal class DataDbTypeConverter
    {
        public static DbVariableType GetDbType(Type type, int length = 0)
        {
            //Type type = cm.UnderlyingType;

            if (type == TypeHelper.Type_String)
                return new DbVariableType(DbFieldType.String, length == 0 ? 250 : length);

            if (type == TypeHelper.Type_Int || type.IsEnum)
                return new DbVariableType(DbFieldType.Int32);

            if (type == TypeHelper.Type_DateTime)
                return new DbVariableType(DbFieldType.DateTime2);

            if (type == TypeHelper.Type_DateTimeOffset)
                return new DbVariableType(DbFieldType.DateTimeOffset);

            //if (type == typeof(DateOnly)) //.net core
            //    return new DbVariableType(DbFieldType.DateTime2);

            if (type == TypeHelper.Type_TimeSpan)
                return new DbVariableType(DbFieldType.Int32);

            if (type == TypeHelper.Type_Boolean)
                return new DbVariableType(DbFieldType.Boolean);

            if (type == TypeHelper.Type_Guid)
                return new DbVariableType(DbFieldType.Guid);

            if (type == TypeHelper.Type_Decimal)
                return new DbVariableType(DbFieldType.Decimal, 20, 8);

            if (type == TypeHelper.Type_Double || type == TypeHelper.Type_Float)
                return new DbVariableType(DbFieldType.Double);

            if (type == TypeHelper.Type_Long || type == TypeHelper.Type_UInt)
                return new DbVariableType(DbFieldType.Int64);

            if (type == TypeHelper.Type_ByteArray)
                return new DbVariableType(DbFieldType.Binary, int.MaxValue);

            if (type == TypeHelper.Type_Byte)
                return new DbVariableType(DbFieldType.Byte);

            if (type == TypeHelper.Type_XmlNode)
                return new DbVariableType(DbFieldType.Xml);

            if (type == TypeHelper.Type_FloatArray)
                return new DbVariableType(DbFieldType.Vector, int.MaxValue);

            return null;
            //throw new BaseNotSupportedException($"Unsupported column type: {type}");
        }
    }
}
