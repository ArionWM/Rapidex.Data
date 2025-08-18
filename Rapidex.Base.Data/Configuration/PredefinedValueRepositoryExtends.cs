//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Rapidex.Data
//{
//    public static class PredefinedValueRepositoryExtends
//    {
//        public static void Register<T>(this IPredefinedValueProcessor rep, Action<T> fillAct) where T : IConcreteEntity, new()
//        {
//            fillAct.NotNull();

//            T instance = new T();
//            fillAct.Invoke(instance);

//            var em = instance.GetMetadata() ?? Database.Metadata.Get(instance.GetType().Name);
//            em.NotNull("Metadata not found");

//            ObjDictionary dict = EntityMapper.MapToDict(em, instance);
//            rep.Register(em, false, dict);
//        }
//    }
//}
