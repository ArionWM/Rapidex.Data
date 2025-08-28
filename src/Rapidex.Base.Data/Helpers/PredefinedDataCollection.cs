using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Helpers;
public class PredefinedDataCollection
{
    public Dictionary<IDbEntityMetadata, PredefinedValueItems> Repository { get; } = new();


    public IDbMetadataContainer Parent { get; }

    public PredefinedDataCollection(IDbMetadataContainer parent)
    {
        this.Parent = parent;
    }


    public void Add(IDbEntityMetadata em, params ObjDictionary[] entities)
    {
        PredefinedValueItems item = this.Repository.Get(em);
        if (item == null)
        {
            item = new PredefinedValueItems(em);
            this.Repository.Add(em, item);
        }

        foreach (var entity in entities)
        {
            item.Entities.Set(entity["Id"].As<long>(), entity);
        }
    }

    public bool Contains(IEntity entity)
    {
        if (entity == null)
            return false;

        IDbEntityMetadata em = entity._Metadata.NotNull();

        PredefinedValueItems item = this.Repository.Get(em);
        if (item == null)
            return false;

        return item.Entities.ContainsKey(entity.GetId().As<long>());
    }


    public void Clear()
    {
        this.Repository.Clear();
    }

    protected void Apply(IDbSchemaScope scope, IDbEntityMetadata em, IEnumerable<IEntity> entities)
    {
        if (em.OnlyBaseSchema && scope.SchemaName != DatabaseConstants.DEFAULT_SCHEMA_NAME)
            return;

        //scope.Structure.ApplyEntityStructure(em, false);

        var availEntities = scope.GetQuery(em).Load();

        using var work = scope.BeginWork();

        entities.ForEach(e =>
        {
            e._IsNew = true;
        });

        if (availEntities.ItemCount == 0)
        {
            entities.Save();
        }
        else
        {
            IDictionary<long, IEntity> dict = availEntities.ToIdDict();

            foreach (var entity in entities)
            {
                var avail = dict.Get(entity.GetId().As<long>());
                //availEntities.FirstOrDefault(e => object.Equals(e.GetId(), entity.GetId()));
                if (avail == null)
                {
                    entity.Save();
                }
                else
                {
                    if (entity.GetValue<bool>(DatabaseConstants.KEY_OVERRIDE))
                    {
                        scope.Mapper.Copy(entity, avail);
                        avail.Save();
                    }
                }
            }
        }
        work.CommitChanges();
    }

    protected void Apply(IDbSchemaScope scope, IDbEntityMetadata em)
    {
        PredefinedValueItems item = this.Repository.Get(em);
        if (item == null)
            return;

        if (em.OnlyBaseSchema && scope.SchemaName != DatabaseConstants.DEFAULT_SCHEMA_NAME)
            return;

        var newEntities = scope.Mapper.Map(em, item.Entities.Values); //.Clone(scope);

        this.Apply(scope, em, newEntities);
    }

    public void Apply(IDbSchemaScope targetSchema)
    {
        foreach (var em in Repository.Keys)
        {
            this.Apply(targetSchema, em);
        }
    }
}
