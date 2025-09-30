using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Rapidex.Data.SerializationAndMapping.JsonConverters;

namespace Rapidex.Data;


//[JsonDerivedBase]
//[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
//[JsonConverter(typeof(RelationOne2NJsonConverter))]
public class RelationOne2N : RelationBase, ILazy
{
    public class VirtualRelationOne2NDbFieldMetadata : Metadata.Columns.VirtualDbFieldMetadata
    {
        public string DetailEntityName { get; set; }
        public string DetailParentFieldName { get; set; }

        public IDbEntityMetadata ReferencedEntityMetadata { get; set; }

        public VirtualRelationOne2NDbFieldMetadata()
        {

        }

        public VirtualRelationOne2NDbFieldMetadata(IDbFieldMetadata source, string referencedEntityName)
        {
            this.Name = source.Name;
            this.Caption = source.Caption ?? referencedEntityName;
            this.DetailEntityName = referencedEntityName;
            this.ParentMetadata = source.ParentMetadata;

            this.IsPersisted = false;
            this.Type = source.Type;

            this.BaseType = typeof(RelationOne2N); //Kullanılmayan değer
            this.DbType = DbFieldType.Binary; //Kullanılmayan değer
            this.SkipDirectLoad = true;
            this.SkipDirectSet = true;
            this.SkipDbVersioning = true;

            this.DetailParentFieldName = "Parent" + this.ParentMetadata.Name;

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
            data.Type = "relationOne2N";
            data.Target = this.DetailEntityName;
            data.AddData("reference", this.DetailEntityName);
            data.AddData("inverseReference", this.DetailParentFieldName);
            //data.AddData("filter", $"{this.DetailParentFieldName} = @parentId");
            string relationInfo = $"{this.ParentMetadata.Name}/@parentId/{this.Name}";
            data.AddData("filter", $"releated = {relationInfo}");
            data.AddData("relationInfo", relationInfo);

            if (this.ReferencedEntityMetadata != null)
            {
                //HasPicture?
                string behaviorNames = this.ReferencedEntityMetadata.Behaviors.Join(",");
                data.Data.Set("targetBehaviors", behaviorNames);
            }
        }


    }

    public virtual string ReferencedEntityName { get; protected set; }

    /// <summary>
    /// Birim testlerinde ClearCache sonrasında otomatik toparlanması için
    /// </summary>
    public virtual string ReferencedEntityConcreteTypeName { get; protected set; }


    public override string TypeName => "relationOne2N";

    [System.Text.Json.Serialization.JsonIgnore]
    public override Type BaseType => typeof(RelationOne2N);

    public override object Clone()
    {
        throw new NotImplementedException();
    }

    object ILazy.GetContent()
    {
        return this.GetContent(null);
    }

    public override IEntityLoadResult GetContent(Action<IQueryCriteria> additionalCriteria = null)
    {
        //Cachlemek mümkün mü? Eğer cache'lenir ise Add'de?
        var fm = (VirtualRelationOne2NDbFieldMetadata)((IDataType)this).FieldMetadata;
        var parentEntity = this.GetParent();
        var detailEm = parentEntity._Schema.ParentDbScope.Metadata.Get(fm.DetailEntityName);
        var detailFm = detailEm.Fields.Get(fm.DetailParentFieldName);
        detailFm.NotNull($"One2N relation detail parent field '{fm.DetailParentFieldName}' not found on '{fm.DetailEntityName}' ");

        var loadResult = parentEntity._Schema.Load(detailEm, crit =>
        {
            crit.Eq(detailFm.Name, parentEntity.GetId());
            additionalCriteria?.Invoke(crit);
        });

        return loadResult;
    }

    public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
    {
        string referencedEntity = values.Value<string>("reference", true);
        this.ReferencedEntityName = referencedEntity;

        VirtualRelationOne2NDbFieldMetadata fm = new VirtualRelationOne2NDbFieldMetadata(self, this.ReferencedEntityName);

        IDbEntityMetadata refMetadata = container.Get(this.ReferencedEntityName);
        if (refMetadata == null)
        {
            container.AddPremature(this.ReferencedEntityName);
            refMetadata = container.Get(this.ReferencedEntityName);
            //refMetadata.ConcreteTypeName = this.ReferencedEntityConcreteTypeName;
        }

        fm.ReferencedEntityMetadata = refMetadata;

        ObjDictionary detailFmValues = new ObjDictionary();
        detailFmValues["name"] = fm.DetailParentFieldName;
        detailFmValues["type"] = "relationOne2N";
        detailFmValues["reference"] = self.ParentMetadata.Name;
        //this.ReferencedEntityName;

        refMetadata.Fields.AddIfNotExist(fm.DetailParentFieldName, "reference", null, detailFmValues);

        return fm;
    }

    public override void Add(IEntity detailEntity)
    {
        //TODO: validate detailEntityType

        IEntity parent = this.GetParent();
        var fm = (VirtualRelationOne2NDbFieldMetadata)((IDataType)this).FieldMetadata;
        long parentId = parent.GetValue<long>(CommonConstants.FIELD_ID);

        detailEntity.SetValue(fm.DetailParentFieldName, parentId);
        detailEntity.Save(); //Save if changed ...
    }
}

/// <summary>
/// Add field ("Parent" + ParentEntityName) to detail entity for parent reference
/// </summary>
/// <typeparam name="TEntity"></typeparam>

//[JsonConverter(typeof(RelationOne2NJsonConverter))]
public class RelationOne2N<TEntity> : RelationOne2N where TEntity : IConcreteEntity
{
    public override string TypeName => "relationOne2NConcrete";

    [System.Text.Json.Serialization.JsonIgnore]
    public override Type BaseType => typeof(RelationOne2N<>);

    public RelationOne2N()
    {
        Type referenceType = typeof(TEntity);
        this.ReferencedEntityName = referenceType.Name;
        this.ReferencedEntityConcreteTypeName = referenceType.FullName;
    }

    public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
    {
        values.Set("reference", typeof(TEntity).Name);
        IDbFieldMetadata fm = base.SetupMetadata(container, self, values);
        fm.Type = this.GetType();
        return fm;
    }

    public override object Clone()
    {
        throw new NotImplementedException();
    }

    public virtual IEntityLoadResult<TEntity> GetContent()
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
