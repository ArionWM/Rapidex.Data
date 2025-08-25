using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Rapidex.Data
{
    [DataContract]
    public class ReferenceDbFieldMetadata : DbFieldMetadata
    {
        [YamlMember(Alias = "reference", Order = 100)]
        [JsonPropertyOrder(100)]
        public string ReferencedEntity { get; set; }

        [JsonIgnore]
        [YamlIgnore]
        public IDbEntityMetadata ReferencedEntityMetadata { get; set; }

        [JsonIgnore]
        [YamlIgnore]
        public Action<IDbSchemaScope, ItemDefinitionExtraData, ReferenceDbFieldMetadata, bool> DefinitionDataCallback;

        public ReferenceDbFieldMetadata()
        {

        }

        public ReferenceDbFieldMetadata(IDbFieldMetadata source)
        {
            this.Name = source.Name;
            this.Caption = source.Caption;
            this.Type = typeof(Reference); //Dikkat, üst sınıf Reference<> olduğunda type'ı Reference değil, ancak burada yine de bu şekilde atıyoruz. Bu şekilde GetValueUpper'da Reference dönüyor.
            this.BaseType = typeof(long);
            this.DbType = DbFieldType.Int64;
            this.SkipDbVersioning = source.SkipDbVersioning;
        }

        public override void GetDefinitionData(IDbSchemaScope scope, ref ItemDefinitionExtraData data, bool placeOptions)
        {
            scope.NotNull();
            data.NotNull();

            base.GetDefinitionData(scope, ref data, placeOptions);
            data.Target = this.ReferencedEntity;

            if (this.ReferencedEntityMetadata != null)
            {
                //HasPicture?
                string behaviorNames = this.ReferencedEntityMetadata.Behaviors.Join(",");
                data.Data.Set("targetBehaviors", behaviorNames);
            }

            this.DefinitionDataCallback?.Invoke(scope, data, this, placeOptions);
        }

    }

    public abstract class ReferenceBase<TThis> : BasicBaseDataType<long, TThis>, ILazy, IReference
        where TThis : ReferenceBase<TThis>, new()
    {
        [YamlMember(Alias = "reference", Order = 100)]
        [JsonPropertyOrder(100)]
        public virtual string ReferencedEntity { get; protected set; }

        /// <summary>
        /// Birim testlerinde ClearCache sonrasında otomatik toparlanması için
        /// </summary>
        public virtual string ReferencedEntityConcreteTypeName { get; protected set; }

        public virtual long TargetId { get { return this.Value; } set { this.Value = value; } }



        public override string TypeName => "reference";
        public override Type BaseType => typeof(long);// typeof(Reference);

        public bool IsEmpty { get { return this.TargetId == DatabaseConstants.DEFAULT_EMPTY_ID || this.TargetId == 0; } }


        public ReferenceBase()
        {
            this.TargetId = DatabaseConstants.DEFAULT_EMPTY_ID;
        }


        public object GetContent()
        {
            IEntity parent = this.GetParent();
            if (parent == null)
                throw new InvalidOperationException("Parent entity is not set");

            if (this.TargetId.IsEmptyId())
                return null;

            if (this.TargetId.IsPrematureId())
                throw new InvalidOperationException("Premature references can't get content");

            IDbSchemaScope scope = parent._Scope;
            IEntity entity = scope.Find(this.ReferencedEntity, this.TargetId)
                .Result;
            return entity;
        }

        public override void SetValue(IEntity entity, string fieldName, object value, bool applyToEntity)
        {
            this.TargetId = DatabaseConstants.DEFAULT_EMPTY_ID;

            bool set = false;

            if (value == null)
            {
                this.Value = default;
                set = true;
            }

            if (value is IEntity targetEntity)
            {
                this.TargetId = (long)targetEntity.GetId();
                set = true;
            }

            if (value is Reference reference)
            {
                this.ReferencedEntity = reference.ReferencedEntity;
                this.TargetId = reference.TargetId;
                set = true;
            }

            if (value is string strVal)
            {
                //Acaba id mi?
                if (long.TryParse(strVal, out long _id))
                {
                    this.TargetId = _id;
                    set = true;
                }
                else
                {
                    //TODO: String gelir ise entity'nin caption field'ı ile aramalı

                }
            }

            if (value is int id)
            {
                this.TargetId = Convert.ToInt64(id);
                set = true;
            }

            if (value is long idlong)
            {
                this.TargetId = idlong;
                set = true;
            }

            if (value is decimal idDecimal) //Json dönüşümünden decimal gelir
            {
                this.TargetId = Convert.ToInt64(idDecimal);
                set = true;
            }

            if (!set)
                throw new InvalidOperationException($"Reference value '{value}' ({value.GetType().Name}) is incorrect");

            if (applyToEntity)
                entity.SetValue(fieldName, this);
        }


        public override IValidationResult Validate()
        {
            throw new NotImplementedException();
        }



        public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
        {
            BasicBaseDataTypeConverter converter = new BasicBaseDataTypeConverter();
            Common.Converter.Register(converter);

            string referencedEntity = values.Value<string>("reference", true);
            this.ReferencedEntity = referencedEntity;

            ReferenceDbFieldMetadata fm = new ReferenceDbFieldMetadata(self);
            fm.Type = this.GetType();

            fm.ValueGetterUpper = this.GetValueUpper;
            fm.ValueSetter = this.SetValue;

            fm.ReferencedEntity = this.ReferencedEntity;

            IDbEntityMetadata refMetadata = container.Get(this.ReferencedEntity);
            if (refMetadata == null)
            {
                container.AddPremature(this.ReferencedEntity);
                refMetadata = container.Get(this.ReferencedEntity);
                //refMetadata.ConcreteTypeName = this.ReferencedEntityConcreteTypeName;
            }

            fm.ReferencedEntityMetadata = refMetadata;
            return fm;
        }

        public override void SetupInstance(IEntity entity, IDbFieldMetadata fm)
        {
            base.SetupInstance(entity, fm);

            ReferenceDbFieldMetadata fmr = (ReferenceDbFieldMetadata)fm;
            this.ReferencedEntity = fmr.ReferencedEntity;
        }

        public override object Clone()
        {
            Reference clone = new Reference();
            clone.ReferencedEntity = this.ReferencedEntity;
            clone.TargetId = this.TargetId;
            return clone;
        }

        public override object GetSerializationData(EntitySerializationOptions options)
        {
            ObjDictionary data = new ObjDictionary();
            data["value"] = this.Value;

            ILazy _this = (ILazy)this;
            IEntity ent = (IEntity)_this.GetContent();
            string caption = ent?.Caption();
            data["text"] = caption;
            return data;
        }

        public override object SetWithSerializationData(string memberName, object value)
        {
            if (value is IDictionary<string, object> dict)
            {
                long _value = dict.Get("value", true).As<long>();
                this.Value = _value;
            }

            return null;
        }

        //public override IPartialEntity[] SetValue(IEntity entity, string fieldName, ObjDictionary value)
        //{
        //    object _value = value.Get("value", true);
        //    this.SetValue(entity, fieldName, _value);

        //    return null;
        //}

    }

    public class Reference : ReferenceBase<Reference>, ILazy, IReference
    {
        public static implicit operator Reference(DbEntity entity)
        {
            Reference reference = new Reference();
            reference.TargetId = (long)entity.GetId();
            reference.ReferencedEntity = entity.GetMetadata().Name;
            return reference;
        }

        public static implicit operator DbEntity(Reference reference)
        {
            IEntity entity = (IEntity)((ILazy)reference).GetContent();
            return (DbEntity)entity;
        }

        public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
        {
            return base.SetupMetadata(container, self, values);
        }
    }


    public class Reference<T> : ReferenceBase<Reference<T>>, ILazy<T>, IReference where T : IEntity
    {
        public override string TypeName => "referenceConcrete";


        public T GetContent()
        {
            object item = ((ILazy)this).GetContent();
            return (T)item;
        }

        public override IValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
        {
            Type referenceType = typeof(T);
            this.ReferencedEntityConcreteTypeName = referenceType.FullName;
            values.Set("reference", referenceType.Name);
            IDbFieldMetadata fm = base.SetupMetadata(container, self, values);
            fm.Type = this.GetType();
            return fm;
        }

        public override object Clone()
        {
            Reference<T> clone = new Reference<T>();
            clone.ReferencedEntity = this.ReferencedEntity;
            clone.ReferencedEntityConcreteTypeName = this.ReferencedEntityConcreteTypeName;
            clone.TargetId = this.TargetId;
            return clone;

        }

        public static implicit operator Reference<T>(DbEntity entity)
        {
            Reference<T> reference = new Reference<T>();
            if (entity == null)
            {
                reference.TargetId = DatabaseConstants.DEFAULT_EMPTY_ID;
            }
            else
            {
                reference.TargetId = (long)entity.GetId();
                reference.ReferencedEntity = entity.GetMetadata().Name;
            }

            return reference;
        }

        public static implicit operator Reference<T>(T entity)
        {
            Reference<T> reference = new Reference<T>();
            if (entity == null)
            {
                reference.TargetId = DatabaseConstants.DEFAULT_EMPTY_ID;
            }
            else
            {
                reference.TargetId = (long)entity.GetId();
                reference.ReferencedEntity = entity.GetMetadata().Name;
            }
            return reference;
        }

        public static implicit operator Reference<T>(long entityId)
        {
            Reference<T> reference = new Reference<T>();
            reference.TargetId = entityId;
            //reference.ReferencedEntityName = entity.GetMetadata().Name;
            return reference;
        }

        public static implicit operator DbEntity(Reference<T> reference)
        {
            IEntity entity = (IEntity)reference.GetContent();
            return (DbEntity)entity;
        }



        public static implicit operator T(Reference<T> reference)
        {
            if (reference == null)
            {
                return default(T);
            }

            IEntity entity = (IEntity)(reference.GetContent() ?? default(T));
            return (T)entity;
        }

    }
}
