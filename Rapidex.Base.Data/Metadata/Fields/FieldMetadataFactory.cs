using System.Data;
using System.Xml;

namespace Rapidex.Data.Metadata
{


    internal class FieldMetadataFactory : IFieldMetadataFactory
    {
        bool started = false;

        Dictionary<string, Type> _fieldTypes = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
        IDbEntityMetadataManager _parent;

        public IDictionary<string, Type> FieldTypes => _fieldTypes;

        public FieldMetadataFactory(IDbEntityMetadataManager parent)
        {
            _parent = parent;
        }

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

        public void Start(IServiceProvider serviceProvider)
        {
            if (started)
                return;

            started = true;
        }


        public void AddType(Type type)
        {
            _fieldTypes.Set(type.Name.CamelCase(), type);

            if (type.IsGenericType && !type.IsConstructedGenericType)
            {
                return;
            }

            if (type.IsSupportTo<IDataType>())
            {
                IDataType dataType = TypeHelper.CreateInstance<IDataType>(type);
                _fieldTypes.Set(dataType.TypeName.CamelCase(), type);
            }
            else
            {
                _fieldTypes.Set(type.Name.CamelCase(), type);
            }
        }

        public void AddType(string typeName, Type type)
        {
            _fieldTypes.Set(typeName.CamelCase(), type);
        }


        public IDbFieldMetadata CreateType(IDbEntityMetadata em, Type type, string name, ObjDictionary values)
        {
            values = values ?? new ObjDictionary();

            DbFieldMetadata fm = new DbFieldMetadata();
            fm.ParentMetadata = em;
            fm.Name = name;
            fm.TypeName = type.Name.CamelCase();

            //Concrete ise sealed olur
            //TODO: fm.Sealed = true;

            if (values != null)
            {
                fm.SkipDbVersioning = values.Get("skipVersioning").As<bool?>() ?? false;
                fm.IsPersisted = values.Get("isPersisted").As<bool?>() ?? true;
                fm.SkipDirectLoad = values.Get("skipDirectLoad").As<bool?>() ?? false;
            }

            DbVariableType dbVariableType = DataDbTypeConverter.GetDbType(type);

            int length = values.Value<int>("lenght");

            if (dbVariableType == null)
            {
                fm.Type = type;

                IDataType dt = TypeHelper.CreateInstance<IDataType>(type);
                IDbFieldMetadata processedFm = dt.NotNull($"Can't create type '{type.Name}'").SetupMetadata(this._parent, fm, values);

                if (dt.SkipDirectLoad.HasValue)
                    processedFm.SkipDirectLoad = dt.SkipDirectLoad.Value;

                if(dt.SkipDirectSet.HasValue)
                    processedFm.SkipDirectSet = dt.SkipDirectSet.Value;

                if (dt.SkipDbVersioning.HasValue)
                    processedFm.SkipDbVersioning = dt.SkipDbVersioning.Value;

                if (processedFm.IsPersisted && (processedFm.Type == null || processedFm.BaseType == null))
                    throw new MetadataException($"{name} DataType metadata definition invalid ({type.Name}, SetupSelfMetadata)");

                if (processedFm.IsPersisted && !processedFm.DbType.HasValue)
                    throw new MetadataException($"{dt} DataType metadata definition invalid ({name}, DbType)");
                return processedFm;
            }
            else
            {
                fm.Type = fm.BaseType = type;
                fm.DbType = dbVariableType.DbType;
                fm.DbProperties.Length = length > 0 ? length : dbVariableType.Lenght;
                fm.DbProperties.Scale = dbVariableType.Scale;

                return fm;
            }
        }

        public IDbFieldMetadata CreateType(IDbEntityMetadata em, Type type, string name)
        {
            string typeName = type.Name;
            ObjDictionary values = new();
            return CreateType(em, type, name, values);
        }

        public IDbFieldMetadata CreateType(IDbEntityMetadata em, string fieldType, string name, ObjDictionary values)
        {
            fieldType = fieldType.ToLowerInvariant();

            switch (fieldType)
            {
                //case "enum":
                //    //Enum zaten concrete bir türdür (şimdilik)
                //    string referenceTypeName = values.NotNull($"'values can't be null for 'enum' definition").Value<string>("reference", true);
                //    Type enumType = Common.Assembly.FindType(referenceTypeName, true);
                //    if (enumType == null)
                //        throw new NotSupportedException($"Enum type '{referenceTypeName}' is not found.");

                //    return CreateType(em, enumType, name, values);

                default:
                    Type type = _fieldTypes.Get(fieldType);
                    if (type == null)
                        type = Common.Assembly.FindType(fieldType, true);

                    if (type == null)
                        throw new NotSupportedException($"Type '{fieldType}' is not supported.");

                    return CreateType(em, type, name, values);
            }
        }
    }
}
