//using Rapidex.Data.Metadata;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Xml.Linq;
//using YamlDotNet.Serialization;

//namespace Rapidex.Data.Behaviors
//{
//    internal class BehavioralEntityMetadata : DbEntityMetadata, IBehavioralEntityMetadata
//    {
        

//        public BehavioralEntityMetadata(string name, string module = null, string prefix = null) : base(name, module, prefix)
//        {
//            Behaviors = new ComponentList<IEntityBehaviorDefinition>();
//            PredefinedListFilters = new ComponentList<IPredefinedListFilter>();
//        }

//        protected void ApplyBehavior(IEntityBehaviorDefinition behavior, bool checkMetadata)
//        {
//            behavior.Setup(this);
//            if (checkMetadata)
//                Database.Metadata.Check(this);
//        }

//        public IBehavioralEntityMetadata AddBehavior<T>(bool @sealed, bool directApply) where T : IEntityBehaviorDefinition
//        {
//            var availableBehavior = this.Behaviors.Get<T>();
//            if (availableBehavior != null)
//                return this;

//            IEntityBehaviorDefinition behavior = TypeHelper.CreateInstance<IEntityBehaviorDefinition>(typeof(T));
//            this.Behaviors.Add(behavior);

//            if (directApply)
//            {
//                this.ApplyBehavior(behavior, true);
//            }
//            return this;
//        }

//        public IBehavioralEntityMetadata AddBehavior(string name, bool @sealed, bool directApply)
//        {
//            var availableBehavior = this.Behaviors.Get(name);
//            if (availableBehavior != null)
//                return this;

//            //TODO: Daha verimli bir yöntem ... Sistemdekileri tutan bir liste
//            Type[] types = Common.Assembly.FindDerivedClassTypes<IEntityBehaviorDefinition>().NotNull();
//            Type type = types.FirstOrDefault(t => t.Name == name).NotNull();

//            IEntityBehaviorDefinition behavior = TypeHelper.CreateInstance<IEntityBehaviorDefinition>(type);
//            this.Behaviors.Add(behavior);

//            if (directApply)
//            {
//                this.ApplyBehavior(behavior, true);
//            }

//            return this;
//        }

//        public IBehavioralEntityMetadata AddPredefinedListFilter<T>() where T : IPredefinedListFilter
//        {
//            throw new NotImplementedException();
//        }

//        public IBehavioralEntityMetadata AddPredefinedListFilter(string name)
//        {
//            throw new NotImplementedException();
//        }

//        public IBehavioralEntityMetadata RemoveBehavior<T>() where T : IEntityBehaviorDefinition
//        {
//            throw new NotImplementedException();
//        }

//        public void ApplyBehaviors()
//        {
//            foreach (IEntityBehaviorDefinition behavior in this.Behaviors.GetAll())
//            {
//                this.ApplyBehavior(behavior, false);
//            }

//            Database.Metadata.Check(this);
//        }

//        public bool Is<T>() where T : IEntityBehaviorDefinition
//        {
//            //TODO: Daha verimli bir yöntem ... Cache vs?
//            return this.Behaviors.List.Any(b => b.IsSupportTo<T>());
//        }
//    }
//}
