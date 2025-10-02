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


    protected IDbMetadataContainer Parent { get; set; }
    protected IFieldMetadataFactory FieldMetadataFactory { get; }
    protected IDbEntityMetadataFactory EntityMetadataFactory { get; set; }
    
    public ComponentDictionary<IDbEntityMetadata> Entities { get; } = new();

    public EntityMetadataBuilderBase(IDbMetadataContainer parent, IDbEntityMetadataFactory dbEntityMetadataFactory, IFieldMetadataFactory fieldMetadataFactory)
    {
        this.SetParent(parent);
        this.EntityMetadataFactory = dbEntityMetadataFactory.NotNull();
        this.FieldMetadataFactory = fieldMetadataFactory.NotNull();
        this.FieldMetadataFactory.SetParent(parent);
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
        em.Fields.AddIfNotExist<long>(DatabaseConstants.FIELD_ID, DatabaseConstants.FIELD_ID, field => { field.IsSealed = true; });
        em.Fields.AddIfNotExist<string>(DatabaseConstants.FIELD_EXTERNAL_ID, DatabaseConstants.FIELD_EXTERNAL_ID, field => { field.IsSealed = true; });
        em.Fields.AddIfNotExist<int>(DatabaseConstants.FIELD_VERSION, DatabaseConstants.FIELD_VERSION, field => { field.IsSealed = true; });
        em.PrimaryKey = em.Fields.Get(DatabaseConstants.FIELD_ID, true);

        em.TableName = em.Prefix.IsNullOrEmpty() ? em.Name : $"{em.Prefix}_{em.Name}";

        this.CheckCaptionField(em);
    }


    public virtual void Add(IDbEntityMetadata em)
    {
        em.Parent = this.Parent;

        this.Check(em);

        IDbEntityMetadata existingEm = this.Parent.Get(em.Name);
        if (existingEm != null && existingEm.IsPremature)
        {
            this.MergeFieldsWithPremature(em, existingEm);
        }

        this.Parent.Add(em);
    }

}
