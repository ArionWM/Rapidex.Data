using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Rapidex.Data.Metadata
{
    internal class DbEntityMetadata : IDbEntityMetadata
    {
        [System.Text.Json.Serialization.JsonIgnore]
        [YamlIgnore]
        public IDbMetadataContainer Parent { get; set; }

        [YamlMember(Order = -9999)]
        [JsonPropertyOrder(-9999)]
        public string Name { get; set; }

        public string NavigationName => this.Name;



        [YamlMember(Order = -9997)]
        [JsonPropertyOrder(-9997)]
        public string Prefix { get; set; }

        [YamlMember(Order = -9996)]
        [JsonPropertyOrder(-9996)]
        public string ModuleName { get; set; }

        [YamlMember(Order = -9995)]
        [JsonPropertyOrder(-9995)]
        public string TableName { get; set; }

        [YamlMember(Order = -9990)]
        [JsonPropertyOrder(-9990)]
        public string ConcreteTypeName { get; set; }

        public bool IsPremature { get; set; }

        public bool OnlyBaseSchema { get; set; }

        public CacheOptions CacheOptions { get; set; } = CacheOptions.Default;

        [System.Text.Json.Serialization.JsonIgnore]
        [YamlIgnore]
        public IDbFieldMetadata PrimaryKey { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [YamlIgnore]
        public IDbFieldMetadata Caption { get; set; }

        public List<string> Tags { get; } = new List<string>();

        [System.Text.Json.Serialization.JsonIgnore]
        [YamlIgnore]
        public DbFieldMetadataList Fields { get; protected set; }

        [JsonPropertyOrder(9999)]
        [YamlMember(Alias = "Fields", Order = 9999)]
        public List<IDbFieldMetadata> FieldsList
        {
            get { return this.Fields.Values.ToList(); }
            set
            {
                this.Fields.Clear();
                foreach (var item in value)
                {
                    this.Fields.Add(item.Name, item);
                }
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        [YamlIgnore]
        public ComponentDictionary<IEntityBehaviorDefinition> BehaviorDefinitions { get; }

        [YamlIgnore] //?? Neden?
        public ComponentDictionary<IPredefinedFilter> Filters { get; }


        [YamlMember(Alias = "behaviors")]

        public List<string> Behaviors
        {
            get
            {
                return this.BehaviorDefinitions.List.Select(be => be.Name).ToList();
            }
            set
            {
                this.BehaviorDefinitions.Clear();
                foreach (var item in value)
                {
                    this.AddBehavior(item, false, false);
                }
            }
        }

        public string CaptionField => this.Caption?.Name;

        public DbEntityMetadata()
        {

        }

        public DbEntityMetadata(string name, string module = null, string prefix = null)
        {
            this.Name = name;
            this.ModuleName = module ?? CommonConstants.MODULE_COMMON;
            this.Prefix = prefix?.Trim()?.TrimEnd('_');
            this.TableName = this.Prefix.IsNullOrEmpty() ? this.Name : $"{this.Prefix}_{this.Name}";
            this.Fields = new DbFieldMetadataList(this);
            this.BehaviorDefinitions = new ComponentDictionary<IEntityBehaviorDefinition>();
            this.Filters = new ComponentDictionary<IPredefinedFilter>();
        }

        public override string ToString()
        {
            return this.Name;
        }

        //public void AddFieldIfNotExist(IDbFieldMetadata column)
        //{
        //    column.ParentMetadata = this;
        //    this.Fields.AddIfNotExist(column);
        //    column.Setup(this);
        //}

        public void AddField(IDbFieldMetadata column)
        {
            column.ParentMetadata = this;
            this.Fields.Add(column);
            column.Setup(this);
        }

        protected IUpdateResult ApplyBehavior(IEntityBehaviorDefinition behavior, bool checkMetadata)
        {
            IUpdateResult ures = behavior.SetupMetadata(this);
            if (checkMetadata)
                this.Check();

            return ures;
        }

        public IDbEntityMetadata AddBehavior<T>(bool @sealed, bool directApply) where T : IEntityBehaviorDefinition
        {
            var availableBehavior = this.BehaviorDefinitions.Get<T>();
            if (availableBehavior != null)
                return this;

            IEntityBehaviorDefinition behavior = TypeHelper.CreateInstance<IEntityBehaviorDefinition>(typeof(T));
            behavior.ParentEntity = this;
            this.BehaviorDefinitions.Add(behavior);

            if (directApply)
            {
                this.ApplyBehavior(behavior, true);
            }
            return this;
        }

        public IDbEntityMetadata AddBehavior(string name, bool @sealed, bool directApply)
        {
            var availableBehavior = this.BehaviorDefinitions.Get(name);
            if (availableBehavior != null)
                return this;

            //TODO: Daha verimli bir yöntem ... Sistemdekileri tutan bir liste
            Type[] types = Common.Assembly.FindDerivedClassTypes<IEntityBehaviorDefinition>().NotNull();
            Type type = types.FirstOrDefault(t => string.Compare(t.Name, name, true) == 0).NotNull($"Behavior not found with '{name}'");

            IEntityBehaviorDefinition behavior = TypeHelper.CreateInstance<IEntityBehaviorDefinition>(type);
            behavior.ParentEntity = this;
            this.BehaviorDefinitions.Add(behavior);

            if (directApply)
            {
                this.ApplyBehavior(behavior, true);
            }

            return this;
        }

        public IDbEntityMetadata AddFilter<T>(T filter) where T : IPredefinedFilter
        {
            this.Filters.Add(filter);
            return this;
        }

        [Obsolete("Use AddFilter<T>() instead", true)]
        public IDbEntityMetadata AddFilter(string name)
        {
            throw new NotImplementedException();
        }

        public IDbEntityMetadata RemoveBehavior<T>() where T : IEntityBehaviorDefinition
        {
            throw new NotImplementedException();
        }

        public IUpdateResult ApplyBehaviors()
        {
            UpdateResult ures = new();
            foreach (IEntityBehaviorDefinition behavior in this.BehaviorDefinitions.List)
            {
                ures.MergeWith(this.ApplyBehavior(behavior, false));
            }

            this.Check();

            return ures;
        }

        /// <summary>
        /// Behavior vb. içeriğin Scope'a uygulanması için çağrılır.
        /// </summary>
        /// <param name="scope"></param>
        public void ApplyToScope(IDbSchemaScope scope)
        {
            foreach (IEntityBehaviorDefinition behavior in this.BehaviorDefinitions.List)
            {
                behavior.ApplyToScope(scope, this);
            }
        }


        public bool Is<T>() where T : IEntityBehaviorDefinition
        {
            //TODO: Daha verimli bir yöntem ... Cache vs?
            return this.BehaviorDefinitions.List.Any(b => b.IsSupportTo<T>());
        }
    }
}
