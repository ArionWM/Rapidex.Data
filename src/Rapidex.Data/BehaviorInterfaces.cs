using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data;

public interface IEntityBehaviorInstance
{
    IEntity Entity { get; }
}

public interface IEntityBehaviorDefinition : IDbDefinition, IOrderedComponent
{
    string Descripton { get; }

    string HelpUrl { get; }

    bool IsVisible { get; }

    Type ImplementerType { get; }

    IDbEntityMetadata ParentEntity { get; set; }

    /// <summary>
    /// Entity metadata tanımlarında (yml ya da json)
    /// Implementer'a verilecek içerik hangi anahtar ile ifade ediliyor.
    /// 
    /// Örn: FlowEntity (behavior) > "flowEntity" (config key) > "phases" (predefined records)
    /// </summary>
    string ImplementerConfigKey { get; }

    public IUpdateResult SetupMetadata(IDbEntityMetadata self);

    public void ApplyToScope(IDbSchemaScope scope, IDbEntityMetadata self);

    public IEntityBehaviorInstance CreateInstance(IEntity forEntity);
}

public interface IEntityBehaviorDefinition<T> where T : IEntityBehaviorInstance
{

}

public interface IEntityBehaviorDefinition<T, I>
    where T : IEntityBehaviorInstance
    where I : IImplementer
{

}
public interface IPredefinedFilter : IComponent, IImplementTarget
{

    string Caption { get; set; }
    string Filter { get; set; }
    string Hint { get; set; }
    string Description { get; set; }
    bool IsDefault { get; set; }
}


