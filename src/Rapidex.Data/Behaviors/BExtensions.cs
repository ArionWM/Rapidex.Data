using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data;

public static class BExtensions
{
    //TODO: Çok verimsiz !!! Her defasında bakmaya gerek yok !!
    public static bool Has<Behavior>(this IDbEntityMetadata em) where Behavior : IEntityBehaviorDefinition
    {
        em.NotNull();
        //return em.BehaviorDefinitions.Get<Behavior>() != null;
        return em.BehaviorDefinitions.Values.Any(b => b.IsSupportTo<Behavior>());
    }

    //public static bool Has<Behavior>(this IDbEntityMetadata em) where Behavior : IEntityBehaviorDefinition
    //{
    //    em.NotNull();

    //    return ((IDbEntityMetadata)em).Behaviors.GetAll().Any(b => b.IsSupportTo<Behavior>());
    //}

    public static bool Has<Behavior>(this IEntity entity) where Behavior : IEntityBehaviorDefinition
    {
        entity.NotNull();

        return entity.GetMetadata().Has<Behavior>();
    }

    public static bool HasBehavior(this IEntity entity, string behaviorName)
    {
        entity.NotNull();

        return ((IDbEntityMetadata)entity.GetMetadata()).BehaviorDefinitions.Get(behaviorName) != null;
    }




    public static T Behavior<T>(this IEntity entity)
        where T : IEntityBehaviorDefinition, IEntityBehaviorInstance
    {
        entity.NotNull();

        IDbEntityMetadata em = (IDbEntityMetadata)entity.GetMetadata();
        T behavior = (T)em.BehaviorDefinitions.Get<T>();
        behavior.NotNull($"Entity '{em.Name}' is not have behavior: '{typeof(T).Name}'");
        
        T inst = (T)behavior.CreateInstance(entity);
        return inst;

    }

    public static T B<T>(this IEntity entity)
       where T : IEntityBehaviorDefinition, IEntityBehaviorInstance
    {
        return entity.Behavior<T>();
    }

    public static IEntityBehaviorInstance Behavior(this IEntity entity, string behaviorName)
    {
        entity.NotNull();

        IDbEntityMetadata em = (IDbEntityMetadata)entity.GetMetadata();
        IEntityBehaviorDefinition behavior = em.BehaviorDefinitions.Get(behaviorName);
        behavior.NotNull($"Entity '{em.Name}' is not have behavior: '{behaviorName}'");

        IEntityBehaviorInstance inst = behavior.CreateInstance(entity);
        return inst;
    }



    public static IDbEntityMetadata AddBehavior(this IDbEntityMetadata em, string name, bool @sealed, bool directApply)
    {
        IDbEntityMetadata bem = em.ShouldSupportTo<IDbEntityMetadata>();
        bem.AddBehavior(name, @sealed, directApply);
        return bem;
    }

    public static IDbEntityMetadata AddBehavior<T>(this IDbEntityMetadata em, bool @sealed, bool directApply) where T : IEntityBehaviorDefinition
    {
        IDbEntityMetadata bem = em.ShouldSupportTo<IDbEntityMetadata>();
        bem.AddBehavior<T>(@sealed, directApply);
        return bem;
    }

    public static T Behavior<T>(this IDbEntityMetadata em) where T : IEntityBehaviorDefinition
    {
        IEntityBehaviorDefinition bd = em.BehaviorDefinitions.Get(typeof(T).Name);
        return (T)bd;
    }

    /// <summary>
    /// Behavior
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="em"></param>
    /// <returns></returns>
    public static T B<T>(this IDbEntityMetadata em) where T : IEntityBehaviorDefinition
    {
        return em.Behavior<T>();
    }
}
