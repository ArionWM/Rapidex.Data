using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;

//[JsonDerivedBase]
public abstract class RelationBase : BasicBaseDataType, ILazy, IRelation
{

    public RelationBase()
    {
        this.SkipDbVersioning = true;
        this.SkipDirectLoad = true;
    }

    //public abstract bool IsEmpty { get; }
    public abstract void Add(IEntity entity);
    public abstract IEntityLoadResult GetContent(Action<IQueryCriteria> additionalCriteria = null);

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
