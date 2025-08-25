using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data;

public abstract class EntityBehaviorBase<T>
    : IEntityBehaviorDefinition, IEntityBehaviorInstance
    where T : EntityBehaviorBase<T>, IEntityBehaviorInstance
{
    #region Definition
    public virtual int Index => 1000;

    public virtual string Name { get; }

    public virtual string NavigationName { get { return this.Name.ToInvariant().CamelCase(); } }

    public virtual string Descripton { get; }

    public virtual string HelpUrl { get; }

    public virtual bool IsVisible { get; } = true;

    public virtual Type ImplementerType { get; }
    public virtual string ImplementerConfigKey { get; }

    public virtual IDbEntityMetadata ParentEntity { get; set; }

    public EntityBehaviorBase()
    {
        this.Name = this.GetType().Name;
    }

    public abstract IUpdateResult SetupMetadata(IDbEntityMetadata em);

    public virtual void ApplyToScope(IDbSchemaScope scope, IDbEntityMetadata em)
    {

    }

    public virtual IEntityBehaviorInstance CreateInstance(IEntity forEntity)
    {
        T instance = TypeHelper.CreateInstance<T>(this.GetType(), forEntity);
        instance.ParentEntity = forEntity.GetMetadata();
        return instance;
    }

    #endregion

    #region Instance


    public IEntity Entity { get; }

    public EntityBehaviorBase(IEntity entity)
    {
        this.Entity = entity;

    }

    #endregion
}

public abstract class EntityBehaviorBase<T, I> : EntityBehaviorBase<T>
    where T : EntityBehaviorBase<T, I>, IEntityBehaviorInstance
    where I : IImplementer
{
    public override Type ImplementerType => typeof(I);
    public override string ImplementerConfigKey => "flowEntity";

    protected EntityBehaviorBase() : base()
    {

    }
    public EntityBehaviorBase(IEntity entity) : base(entity)
    {
    }
}