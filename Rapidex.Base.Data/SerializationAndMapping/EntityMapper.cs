using FluentAssertions.Equivalency;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
//using static Rapidex.Data.ConcreteEntityMapper;

namespace Rapidex.Data;

public class EntityMapper
{
    protected IDbSchemaScope Parent { get; }
    public class EntityTypeMap
    {
        public IDbEntityMetadata EntityMetadata { get; set; }
        public List<IDbFieldMetadata> AdvancedFields { get; } = new List<IDbFieldMetadata>();

        public void CreateMap()
        {

            foreach (IDbFieldMetadata fm in this.EntityMetadata.Fields.Values)
            {
                if (fm.Type.IsSupportTo<IDataType>())
                {
                    this.AdvancedFields.Add(fm);
                }
            }
        }
    }

    protected DictionaryA<EntityTypeMap> Index { get; } = new DictionaryA<EntityTypeMap>();

    protected ReaderWriterLockSlim Lock { get; } = new ReaderWriterLockSlim();

    public EntityMapper(IDbSchemaScope parent)
    {
        this.Parent = parent;
    }

    public EntityTypeMap GetMapping(IDbEntityMetadata em)
    {
        this.Lock.EnterUpgradeableReadLock();
        try
        {
            EntityTypeMap etm = this.Index.Get(em.Name);
            if (etm != null)
            {
                return etm;
            }
            else
            {
                this.Lock.EnterWriteLock();
                try
                {
                    EntityTypeMap tpl = new EntityTypeMap();
                    tpl.EntityMetadata = em;
                    tpl.CreateMap();
                    this.Index.Add(em.Name, tpl);
                    return tpl;
                }
                finally
                {
                    this.Lock.ExitWriteLock();
                }
            }
        }
        finally
        {
            this.Lock.ExitUpgradeableReadLock();
        }
    }

    public void EnsureAdvancedDataTypes(IEntity entity)
    {
        entity.Should().NotBeAssignableTo<IPartialEntity>("Partial entities can't be fill");

        var em = entity.GetMetadata();
        var map = this.GetMapping(em);

        foreach (IDbFieldMetadata fm in map.AdvancedFields)
        {
            object value = entity.GetValue(fm.Name);
            IDataType available = value as IDataType;

            //EnsureValueType?
            if (available == null)
            {
                var dt = TypeHelper.CreateInstance<IDataType>(fm.Type);
                dt.SetupInstance(entity, fm);

                if (value != null)
                {
                    dt.SetValue(entity, fm.Name, value, true);
                }

                entity.SetValue(fm.Name, dt);

            }
            else
            {
                available.SetupInstance(entity, fm);
            }
        }
    }

    public static IDataType CreateDataTypeField(IDbFieldMetadata fm, IEntity parent)
    {
        fm.Type.ShouldSupportTo<IDataType>($"Required data type {fm.Type} (for {fm.Name}) is not IDataType");
        IDataType dt = TypeHelper.CreateInstance<IDataType>(fm.Type);
        dt.SetupInstance(parent, fm);
        return dt;
    }

    public static object EnsureValueType(IDbFieldMetadata fm, IEntity parent, object value, bool applyToEntity = true)
    {
        if (value != null && value.IsSupportTo(fm.Type))
            return value;

        if (value is JsonElement jelement)
        {
            value = jelement.GetValueAsOriginalType();
        }

        if (fm.Type.IsSupportTo<IDataType>())
        {
            IDataType dt = CreateDataTypeField(fm, parent);
            dt.SetValue(parent, fm.Name, value, applyToEntity);
            return dt;

        }
        else
        {
            return value.As(fm.Type);
        }

    }

    public static object EnsureValueType(PropertyInfo pinfo, IEntity parent, object value, bool applyToEntity = true)
    {
        if (value != null && value.IsSupportTo(pinfo.PropertyType))
            return value;


        if (pinfo.PropertyType.IsSupportTo<IDataType>())
        {
            var em = parent.GetMetadata();
            var fm = em.Fields[pinfo.Name];

            if (fm.SkipDirectSet)
                return value;

            IDataType dt = CreateDataTypeField(fm, parent);
            dt.SetValue(parent, pinfo.Name, value, applyToEntity);
            return dt;
        }
        else
        {
            return value.As(pinfo.PropertyType);
        }

    }

    public IEntity Clone(IEntity from, IDbSchemaScope targetScope = null)
    {
        var em = from.GetMetadata();
        IEntity clone = Database.EntityFactory.Create(this.Parent, em, false);

        if (targetScope == null)
        {
            clone._DbName = from._DbName;
            clone._Scope = from._Scope;
            clone._SchemaName = from._SchemaName;
            clone.SetId((long)from.GetId());
            clone.DbVersion = from.DbVersion;
        }
        else
        {
            clone._DbName = targetScope.ParentDbScope.Name;
            clone._Scope = targetScope;
            clone._SchemaName = targetScope.SchemaName;
            clone._IsNew = true;
            clone.SetId(0L);
        }


        return this.Copy(from, clone);
    }


    public IEntity Copy(IEntity from, IEntity to)
    {
        var em = from.GetMetadata();

        to._TypeName = from._TypeName;
        to.ExternalId = from.ExternalId;

        foreach (var fm in em.Fields.Values)
        {
            if (fm == em.PrimaryKey)
                continue;

            if (fm.SkipDirectSet)
                continue;

            object val = fm.ValueGetterUpper(from, fm.Name);

            if (val is IDataType dt)
            {
                val = dt.CloneFor(to, fm);
            }

            to.SetValue(fm.Name, val);
        }

        //var values = from.GetAllValues();

        //foreach (var kv in values)
        //{
        //    IDbFieldMetadata fm = em.Fields[kv.Key];
        //    object val = kv.Value;


        //    if (val is IDataType dt)
        //    {
        //        val = dt.CloneFor(to, fm);
        //    }

        //    to[kv.Key] = val;
        //}

        this.EnsureAdvancedDataTypes(to);

        return to;
    }

    public IEntity Map(IDbEntityMetadata em, DataRow from, IEntity fillTo)
    {
        foreach (IDbFieldMetadata fm in em.Fields.Values)
        {
            if (!fm.IsPersisted)
                continue;

            if (fm.SkipDirectLoad)
                continue;

            object value = null;

            value = from[fm.Name];
            if (Convert.IsDBNull(value))
                value = null;

            value = EnsureValueType(fm, fillTo, value);

            fillTo.SetValue(fm.Name, value);
        }

        fillTo._Scope = this.Parent;
        fillTo.SetId(em.PrimaryKey.ValueGetterLower(fillTo, em.PrimaryKey.Name));

        return fillTo;
    }

    public IEntity MapToNew(IDbEntityMetadata em, DataRow from)
    {
        IEntity instance = Database.EntityFactory.Create(this.Parent, em, false);
        return this.Map(em, from, instance);
    }


    public IEntity[] MapToNew(IDbEntityMetadata em, DataTable table)
    {
        List<IEntity> entities = new List<IEntity>();
        foreach (DataRow row in table.Rows)
        {
            entities.Add(this.MapToNew(em, row));
        }

        return entities.ToArray();
    }
    public IPartialEntity Map(IDbEntityMetadata em, DataRow from, IPartialEntity fillTo, string[] fields)
    {
        foreach (string fieldName in fields)
        {
            IDbFieldMetadata fm = em.Fields.Get(fieldName);
            if (!fm.IsPersisted)
                continue;

            if (fm.SkipDirectLoad)
                continue;

            if (fields.IsNOTNullOrEmpty() && !fields.Contains(fm.Name, StringComparer.InvariantCultureIgnoreCase))
            {
                continue;
            }

            object value = null;

            value = from[fm.Name];
            if (Convert.IsDBNull(value))
                value = null;

            value = EnsureValueType(fm, fillTo, value);

            fillTo.SetValue(fm.Name, value);
        }

        fillTo.SetValue(CommonConstants.FIELD_ID, from[CommonConstants.FIELD_ID]);
        fillTo.SetValue(CommonConstants.FIELD_VERSION, from[CommonConstants.FIELD_VERSION]);

        fillTo._Scope = this.Parent;
        fillTo.SetId(em.PrimaryKey.ValueGetterLower(fillTo, em.PrimaryKey.Name));

        return fillTo;
    }


    public IEntity Map(IDbEntityMetadata em, IDictionary<string, object> from, IEntity fillTo)
    {
        fillTo.ShouldNotSupportTo<IPartialEntity>("Partial entities can't be map");

        foreach (IDbFieldMetadata fm in em.Fields.Values)
        {
            if (!fm.IsPersisted)
                continue;

            if (fm.SkipDirectLoad)
                continue;

            object value = from.Get(fm.Name);
            if (Convert.IsDBNull(value))
                value = null;

            value = EnsureValueType(fm, fillTo, value);

            fillTo.SetValue(fm.Name, value);
        }

        fillTo._Scope = this.Parent;
        fillTo.SetId(em.PrimaryKey.ValueGetterLower(fillTo, em.PrimaryKey.Name));

        return fillTo;
    }

    public IEntity MapToNew(IDbEntityMetadata em, IDictionary<string, object> from)
    {
        IEntity instance = Database.EntityFactory.Create(this.Parent, em, true);
        return this.Map(em, from, instance);
    }

    public static ObjDictionary MapToDict(IDbEntityMetadata em, IEntity entity)
    {
        ObjDictionary dict = new ObjDictionary();

        foreach (IDbFieldMetadata fm in em.Fields.Values)
        {
            if (!fm.IsPersisted)
                continue;

            object value = entity.GetValue(fm.Name);
            if (value is IDataType dt)
            {
                value = dt.Clone();
            }

            dict.Add(fm.Name, value);
        }
        return dict;
    }

    public static IDictionary<string, object> MapToDict(IEntity entity)
    {
        var em = entity.GetMetadata() ?? Database.Metadata.Get(entity.GetType().Name);
        em.NotNull();
        return EntityMapper.MapToDict(em, entity);
    }
    public static IEnumerable<IDictionary<string, object>> MapToDict(IEnumerable<IEntity> entities)
    {
        if (entities.IsNullOrEmpty())
        {
            return new IDictionary<string, object>[0];
        }

        var em = entities.First().GetMetadata();
        return entities.Select(e => EntityMapper.MapToDict(em, e));
    }

    public T Map<T>(IEntity source) where T : IConcreteEntity
    {
        Type conctType = typeof(T);

        T concEntity = (T)Database.EntityFactory.Create<T>(source._Scope, false);

        concEntity._IsNew = source._IsNew;
        concEntity.Id = (long)source.GetId();
        concEntity.ExternalId = source.ExternalId;
        concEntity.DbVersion = source.DbVersion;

        this.Copy(source, concEntity);


        /*
            object val = fm.ValueGetterUpper(from, fm.Name);

            if (val is IDataType dt)
            {
                val = dt.CloneFor(to, fm);
            }

            to.SetValue(fm.Name, val);             
         */

        //TypeParameterList typeParameterList = this.GetMapping(conctType);

        //foreach (var prop in typeParameterList.Properties)
        //{
        //    var value = source.GetValue(prop.Name);

        //    concEntity.SetValue(prop.Name, value);
        //}

        concEntity.EnsureDataTypeInitialization();

        return concEntity;
    }
}
