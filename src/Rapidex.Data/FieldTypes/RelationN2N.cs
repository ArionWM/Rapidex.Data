using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Rapidex.Data.Metadata.Relations;
using static Rapidex.Data.RelationOne2N;

namespace Rapidex.Data;

//[JsonDerivedBase]
public class RelationN2N : RelationBase, ILazy
{
    public class VirtualRelationN2NDbFieldMetadata : Metadata.Columns.VirtualDbFieldMetadata
    {
        public string TargetEntityName { get; set; }
        //public IDbEntityMetadata TargetEntityMetadata { get; set; }
        public string JunctionEntityName { get; set; } = JunctionHelper.DEFAULT_JUNCTION_ENTITY_NAME;

        public string JunctionSourceFieldName { get; set; }
        public string JunctionTargetFieldName { get; set; }

        //Kendine referans?

        //public IDbEntityMetadata TargetEntityMetadata { get; set; }

        public VirtualRelationN2NDbFieldMetadata()
        {

        }

        public VirtualRelationN2NDbFieldMetadata(IDbFieldMetadata source, string targetEntityName)
        {
            this.Name = source.Name;
            this.Caption = source.Caption ?? targetEntityName;
            this.TargetEntityName = targetEntityName;

            this.ParentMetadata = source.ParentMetadata;

            this.JunctionSourceFieldName = this.ParentMetadata.Name.ToFriendly();
            this.JunctionTargetFieldName = this.TargetEntityName.ToFriendly();

            this.IsPersisted = false;
            this.Type = source.Type;

            this.BaseType = typeof(RelationN2N); //Kullanılmayan değer
            this.DbType = DbFieldType.Binary; //Kullanılmayan değer
            this.SkipDirectLoad = true;
            this.SkipDirectSet = true;
            this.SkipDbVersioning = true;

            this.ValueSetter = (entity, fieldName, value, applyToEntity) =>
            {
                throw new NotSupportedException();
            };

            this.ValueGetterUpper = (entity, fieldName) =>
            {
                return BasicBaseDataType.GetValueUpperSt(entity, fieldName);
            };

            this.ValueGetterLower = (entity, fieldName) =>
            {
                return null;
            };

        }

        public override void Setup(IDbEntityMetadata parentMetadata)
        {
            base.Setup(parentMetadata);
        }

        public override void GetDefinitionData(IDbSchemaScope scope, ref ItemDefinitionExtraData data, bool placeOptions)
        {
            base.GetDefinitionData(scope, ref data, placeOptions);
            data.Target = this.TargetEntityName;
            data.Type = "relationN2N";
            data.AddData("reference", this.TargetEntityName);
            data.AddData("junction", null); //GenericJunction

            string relationInfo = $"{this.ParentMetadata.Name}/@parentId/{this.Name}";
            data.AddData("filter", $"releated = {relationInfo}");
            data.AddData("relationInfo", relationInfo);

            var targetEm = scope.ParentDbScope.Metadata.Get(this.TargetEntityName)
                 .NotNull($"Target entity metadata not found: {this.TargetEntityName}");

            string behaviorNames = targetEm.Behaviors.Join(",");
            data.Data.Set("targetBehaviors", behaviorNames);
        }
    }

    public virtual string TargetEntityName { get; protected set; }

    /// <summary>
    /// Birim testlerinde ClearCache sonrasında otomatik toparlanması için
    /// </summary>
    public virtual string TargetEntityConcreteTypeName { get; protected set; }


    public override string TypeName => "relationN2N";

    [System.Text.Json.Serialization.JsonIgnore]
    public override Type BaseType => typeof(RelationN2N);

    public override object Clone()
    {
        throw new NotImplementedException();
    }

    public virtual void SetContentCriteria(IQueryCriteria query, Action<IQueryCriteria> additionalCriterias = null)
    {
        var fm = (VirtualRelationN2NDbFieldMetadata)((IDataType)this).FieldMetadata;
        var parentEntity = this.GetParent();

        JunctionHelper.SetEntitiesCriteria(parentEntity._Schema, fm, parentEntity, query, additionalCriterias);
    }

    public override IEntityLoadResult GetContent(Action<IQueryCriteria> additionalCriteria = null)
    {
        var fm = (VirtualRelationN2NDbFieldMetadata)((IDataType)this).FieldMetadata;
        var parentEntity = this.GetParent();

        IEntityLoadResult res = JunctionHelper.GetEntities(parentEntity._Schema, fm, parentEntity, additionalCriteria);
        return res;
    }

    

    public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
    {
        string referencedEntity = values.Value<string>("reference", true);
        this.TargetEntityName = referencedEntity;

        VirtualRelationN2NDbFieldMetadata fm = new VirtualRelationN2NDbFieldMetadata(self, this.TargetEntityName);

        IDbEntityMetadata refMetadata = container.Get(this.TargetEntityName);
        if (refMetadata == null)
            container.AddPremature(this.TargetEntityName);

        refMetadata = container.Get(this.TargetEntityName);

        JunctionHelper.AddJunctionFields(fm);

        return fm;
    }

    public override void Add(IEntity detailEntity)
    {
        //TODO: validate detailEntityType

        IDataType _this = this;
        IEntity parent = this.GetParent();
        var fm = (VirtualRelationN2NDbFieldMetadata)((IDataType)this).FieldMetadata;

        JunctionHelper.AddRelation(_this.Parent._Schema, fm, parent, detailEntity, true);
    }

    public void Remove(IEntity detailEntity)
    {
        IDataType _this = this;
        IEntity parent = this.GetParent();
        var fm = (VirtualRelationN2NDbFieldMetadata)((IDataType)this).FieldMetadata;

        JunctionHelper.RemoveRelation(_this.Parent._Schema, fm, parent, detailEntity, true);
    }
}

public class RelationN2N<TEntity> : RelationN2N where TEntity : IConcreteEntity
{
    public override string TypeName => "relationN2NConcrete";

    [System.Text.Json.Serialization.JsonIgnore]
    public override Type BaseType => typeof(RelationN2N<>);

    public RelationN2N()
    {
        Type referenceType = typeof(TEntity);
        this.TargetEntityName = referenceType.Name;
        this.TargetEntityConcreteTypeName = referenceType.FullName;
    }

    public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
    {
        values.Set("reference", typeof(TEntity).Name);
        IDbFieldMetadata fm = base.SetupMetadata(container, self, values);
        fm.Type = this.GetType();
        return fm;
    }

    public virtual new IEntityLoadResult<TEntity> GetContent(Action<IQueryCriteria> additionalCriteria = null)
    {
        IEntityLoadResult details = (IEntityLoadResult)((ILazy)this).GetContent();
        IEntityLoadResult<TEntity> cdetails = details.CastTo<TEntity>();
        return cdetails;
    }

    public void Add(TEntity detailEntity)
    {
        base.Add(detailEntity);
    }

}
