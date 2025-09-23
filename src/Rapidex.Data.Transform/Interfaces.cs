using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Rapidex.Data.Transform;

namespace Rapidex.Data;
public interface IEntitySerializationDataCreator
{
    object ConvertToFieldData(IEntity entity, IDbFieldMetadata fm);
    T ConvertToEntityData<T>(IEntity entity, EntitySerializationOptions options, params string[] fields) where T : EntityDataDto, new();
    EntityDataDto ConvertToEntityData(IEntity entity, EntitySerializationOptions options, params string[] fields);

    EntityDataDtoCollection<T> ConvertToListData<T>(IEnumerable<IEntity> entities, EntitySerializationOptions options, Dictionary<string, object> properties, params string[] fields) where T : EntityDataDto, new();
    EntityDataDtoCollection<EntityDataDto> ConvertToListData(IEnumerable<IEntity> entities, EntitySerializationOptions options, Dictionary<string, object> properties, params string[] fields);
}

