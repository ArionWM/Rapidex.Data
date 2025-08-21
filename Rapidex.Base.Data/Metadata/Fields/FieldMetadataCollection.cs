using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Rapidex.Data.Metadata;
internal class FieldMetadataCollection
{
    static Dictionary<string, Type> fieldTypes = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

    public static IDictionary<string, Type> FieldTypes => fieldTypes;

    public void Setup(IServiceCollection services)
    {
        this.AddType("byte", typeof(byte));
        this.AddType("short", typeof(Int16));
        this.AddType("int", typeof(int));
        this.AddType("int32", typeof(int));
        this.AddType("int64", typeof(long));
        this.AddType("long", typeof(long));
        this.AddType("string", typeof(string));
        this.AddType("datetime", typeof(DateTimeOffset));
        this.AddType("date", typeof(DateTimeOffset));
        this.AddType("time", typeof(DateTimeOffset));
        this.AddType("datetimediff", typeof(TimeSpan));
        this.AddType("timespan", typeof(TimeSpan));
        this.AddType("boolean", typeof(bool));
        this.AddType("bool", typeof(bool));
        this.AddType("yesno", typeof(bool));
        this.AddType("guid", typeof(Guid));
        this.AddType("decimal", typeof(decimal));
        this.AddType("double", typeof(double));
        this.AddType("float", typeof(double));
        this.AddType("byte[]", typeof(byte[]));
        this.AddType("binary", typeof(byte[]));
        this.AddType("xml", typeof(XmlNode));

        var types = Common.Assembly.FindDerivedClassTypes<IDataType>();
        foreach (var type in types)
        {
            AddType(type);
        }
    }

    public void AddType(Type type)
    {
        fieldTypes.Set(type.Name.CamelCase(), type);

        if (type.IsGenericType && !type.IsConstructedGenericType)
        {
            return;
        }

        if (type.IsSupportTo<IDataType>())
        {
            IDataType dataType = TypeHelper.CreateInstance<IDataType>(type);
            fieldTypes.Set(dataType.TypeName.CamelCase(), type);
        }
        else
        {
            fieldTypes.Set(type.Name.CamelCase(), type);
        }
    }

    public void AddType(string typeName, Type type)
    {
        fieldTypes.Set(typeName.CamelCase(), type);
    }


}
