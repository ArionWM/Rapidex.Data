using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;

public class EntitySerializationDataCreator : IEntitySerializationDataCreator
{


    public virtual object ConvertToFieldData(IEntity entity, IDbFieldMetadata fm)
    {
        entity.NotNull();
        fm.NotNull();

        object upperValue = fm.ValueGetterUpper(entity, fm.Name);
        if (upperValue is ISerializationDataProvider sdp)
        {
            object data = sdp.GetSerializationData(EntitySerializationOptions.Default); //fm.Name, 
            return data;
        }

        object lowerValue = fm.ValueGetterLower(entity, fm.Name);
        return lowerValue;
    }


    public virtual T ConvertToEntityData<T>(IEntity entity, EntitySerializationOptions options, params string[] fields) where T : EntityDataDtoBase, new()
    {
        entity.NotNull();

        IDbEntityMetadata em = entity.GetMetadata();

        T eddto = new T();

        eddto.Entity = em.Name;

        eddto[CommonConstants.FIELD_ID] = entity.GetId();
        eddto[CommonConstants.FIELD_VERSION] = entity.DbVersion;

        if (options.IncludeBaseFields)
        {
            eddto[CommonConstants.DATA_FIELD_ID] = entity.GetId();
            eddto[CommonConstants.DATA_FIELD_CAPTION] = entity.Caption();
        }

        if (options.IncludeTypeName)
        {
            eddto[CommonConstants.DATA_FIELD_TYPENAME] = em.Name;
        }

        if (options.IncludePictureField)
        {
            ////CommonConstants.DATA_FIELD_PICTURE
            ////string nav = Image.GetFileDescriptorIdForFieldFile(owner, em, fieldName);
            ////GetFileDescriptorIdForFieldFile
            //throw new NotImplementedException();
        }

        foreach (IDbFieldMetadata fm in em.Fields.Values)
        {
            if (fields.IsNOTNullOrEmpty() && !fields.Contains(fm.Name, StringComparer.InvariantCultureIgnoreCase))
            {
                continue;
            }

            object value = ConvertToFieldData(entity, fm);
            eddto[fm.Name] = value;
        }

        return eddto;
    }

    public virtual EntityDataDtoBase ConvertToEntityData(IEntity entity, EntitySerializationOptions options, params string[] fields)
    {
        return this.ConvertToEntityData<EntityDataDtoBase>(entity, options, fields);
    }

    public virtual ListDataDtoCollection<T> ConvertToListData<T>(IEnumerable<IEntity> entities, EntitySerializationOptions options, Dictionary<string, object> properties, params string[] fields) where T : EntityDataDtoBase, new()
    {
        List<T> list = new List<T>();
        foreach (IEntity entity in entities)
        {
            T eddto = ConvertToEntityData<T>(entity, options, fields);
            list.Add(eddto);
        }

        ListDataDtoCollection<T> coll = new ListDataDtoCollection<T>(list);
        coll.Properties = properties;

        return coll;
    }

    public virtual ListDataDtoCollection<EntityDataDtoBase> ConvertToListData(IEnumerable<IEntity> entities, EntitySerializationOptions options, Dictionary<string, object> properties, params string[] fields)
    {
        return this.ConvertToListData<EntityDataDtoBase>(entities, options, properties, fields);
    }
}
