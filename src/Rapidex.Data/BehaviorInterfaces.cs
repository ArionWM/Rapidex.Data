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


[Obsolete("Use IPredefinedFilter instead", true)]
public interface IPredefinedListFilter : IOrderedComponent
{

    string DisplayName { get; set; }

    string Filter { get; set; }
}


//public interface IBehavioralEntityMetadata : IDbEntityMetadata
//{

//    ComponentList<IEntityBehaviorDefinition> Behaviors { get; }
//    ComponentList<IPredefinedListFilter> PredefinedListFilters { get; }


//    /// <summary>
//    /// 
//    /// </summary>
//    /// <typeparam name="T"></typeparam>
//    /// <param name="sealed">True: Definition can't change by administrators</param>
//    IBehavioralEntityMetadata AddBehavior<T>(bool @sealed, bool directApply) where T : IEntityBehaviorDefinition;

//    IBehavioralEntityMetadata AddBehavior(string name, bool @sealed, bool directApply);

//    IBehavioralEntityMetadata AddPredefinedListFilter<T>() where T : IPredefinedListFilter;

//    IBehavioralEntityMetadata AddPredefinedListFilter(string name);


//    IBehavioralEntityMetadata RemoveBehavior<T>() where T : IEntityBehaviorDefinition;

//    bool Is<T>() where T : IEntityBehaviorDefinition;

//    void ApplyBehaviors();
//}
