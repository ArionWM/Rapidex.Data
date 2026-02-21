using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;

public abstract class RelationBase : BasicBaseDataType, ILazy, IRelation
{
    protected HashSet<IEntity> addedEntities = new();
    protected HashSet<IEntity> removedEntities = new();


    public RelationBase()
    {
        this.SkipDbVersioning = true;
        this.SkipDirectLoad = true;
    }

    public abstract IEntity[] GetContent(Action<IQueryCriteria> additionalCriteria = null);
    public abstract void Add(IEntity detailEntity);
    public abstract void Remove(IEntity detailEntity);

    object ILazy.GetContent()
    {
        return this.GetContent(null);
    }

    public override object GetValueLower()
    {
        throw new NotSupportedException();
    }

    public override object GetValueUpper(IEntity entity, string fieldName)
    {
        throw new NotSupportedException();
    }

    public override void SetValue(IEntity entity, string fieldName, object value, bool applyToEntity)
    {
        throw new NotSupportedException();
    }

    public override IPartialEntity[] SetValue(IEntity entity, string fieldName, ObjDictionary value)
    {
        throw new NotImplementedException("!!!!");
    }



    public override void SetValuePremature(object value)
    {
        throw new NotSupportedException();
    }

    public override IValidationResult Validate()
    {
        throw new NotImplementedException();
    }

    

}
