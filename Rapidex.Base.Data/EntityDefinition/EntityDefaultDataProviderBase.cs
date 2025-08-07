//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Rapidex.Data
//{
//    public abstract class EntityDefaultDataProviderBase : IEntityDefaultDataProvider
//    {
//        public abstract string Entity { get; }

//        public abstract void Apply();
//    }


//    public abstract class EntityDefaultDataProviderBase<TEntity> : EntityDefaultDataProviderBase where TEntity : IEntity
//    {
//        public override string Entity => typeof(TEntity).Name;
//    }

//}