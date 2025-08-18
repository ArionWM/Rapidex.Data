using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Metadata;
internal abstract class EntityMetadataBuilderBase
{
    public string ModuleName { get; }


    public IDbMetadataContainer Parent { get; protected set; }
    public IFieldMetadataFactory FieldMetadataFactory { get; }
    public IDbEntityMetadataFactory EntityMetadataFactory { get; protected set; }
    public EntityMetadataBuilderFromEnum EnumerationDefinitionFactory { get; }
    public ComponentDictionary<IDbEntityMetadata> Entities { get; } = new();

    //public EntityMetadataBuilderBase(IDbEntityMetadataFactory dbEntityMetadataFactory, IFieldMetadataFactory fieldMetadataFactory)
    //{
    //}

    public EntityMetadataBuilderBase(IDbMetadataContainer parent, IDbEntityMetadataFactory dbEntityMetadataFactory, IFieldMetadataFactory fieldMetadataFactory)
    {
        this.SetParent(parent);
        this.EntityMetadataFactory = dbEntityMetadataFactory.NotNull();
        this.FieldMetadataFactory = fieldMetadataFactory.NotNull();
        this.EnumerationDefinitionFactory = new EntityMetadataBuilderFromEnum(this.Parent, this.EntityMetadataFactory, this.FieldMetadataFactory);

    }

    protected virtual void Validate()
    {
        this.Parent.NotNull("Parent can't be null. Use SetParent() method to set the parent  before using this implementer.");
    }

    protected virtual void SetParent(IDbMetadataContainer parent)
    {
        parent.NotNull();
        this.Parent = parent;
    }




    public static string[] CaptionFieldNames = new string[] { "Caption", "Title", "Subject", "FullName", "Name" };
    protected virtual void CheckCaptionField(IDbEntityMetadata em)
    {
        foreach (string captionFieldName in CaptionFieldNames)
        {
            if (em.Fields.ContainsKey(captionFieldName))
            {
                em.Caption = em.Fields[captionFieldName];
                break;
            }
        }
    }

    protected virtual void CheckEnumerations(IDbEntityMetadata em)
    {
        foreach (var field in em.Fields.Values)
        {
            if (field.Type.IsEnum)
            {
                throw new MetadataException($"{em.Name} / {field.Name} is system.Enum. Should convert to Enumeration<Enum>.");
            }

            if (field.Type.IsSupportTo<Enumeration>())
            {
                if (field.Type.IsGenericType)
                {
                    Type enumType = field.Type.GetGenericArguments()[0];
                    //Log.Debug(string.Format("Enumeration field: {0} / {1} / {}", em.Name, field.Name, enumType.Name));

                    this.EnumerationDefinitionFactory.Add(enumType);
                }
            }
        }
    }

    protected virtual void MergeFieldsWithPremature(IDbEntityMetadata em, IDbEntityMetadata premature)
    {
        foreach (IDbFieldMetadata field in premature.Fields.Values)
        {
            if (!em.Fields.ContainsKey(field.Name))
            {
                em.AddField(field);
            }
        }
    }

    public virtual void Check(IDbEntityMetadata em)
    {
        em.Fields.AddIfNotExist<long>(CommonConstants.FIELD_ID, CommonConstants.FIELD_ID, field => { field.IsSealed = true; });
        em.Fields.AddIfNotExist<string>(CommonConstants.FIELD_EXTERNAL_ID, CommonConstants.FIELD_EXTERNAL_ID, field => { field.IsSealed = true; });
        em.Fields.AddIfNotExist<int>(CommonConstants.FIELD_VERSION, CommonConstants.FIELD_VERSION, field => { field.IsSealed = true; });
        em.PrimaryKey = em.Fields.Get(CommonConstants.FIELD_ID, true);

        em.TableName = em.Prefix.IsNullOrEmpty() ? em.Name : $"{em.Prefix}_{em.Name}";

        this.CheckCaptionField(em);
        this.CheckEnumerations(em);
    }


    public virtual void Add(IDbEntityMetadata em)
    {
        this.Check(em);

        IDbEntityMetadata existingEm = this.Parent.Get(em.Name);
        if (existingEm != null && existingEm.IsPremature)
        {
            this.MergeFieldsWithPremature(em, existingEm);
        }

        this.Parent.Add(em);
    }

}
