using Rapidex.Base.Common.Assemblies;
using Rapidex.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Metadata;
internal class EntityMetadataBuilderFromConcrete : EntityMetadataBuilderBase
{
    public EntityMetadataBuilderFromConcrete(IDbEntityMetadataFactory dbEntityMetadataFactory, IFieldMetadataFactory fieldMetadataFactory) : base(dbEntityMetadataFactory, fieldMetadataFactory)
    {
    }

    public EntityMetadataBuilderFromConcrete(IDbEntityMetadataFactory dbEntityMetadataFactory, IFieldMetadataFactory fieldMetadataFactory, IDbMetadataContainer parent) : base(dbEntityMetadataFactory, fieldMetadataFactory, parent)
    {
    }

    protected virtual void ValidateConcreteType(Type type)
    {
        if (type.IsInterface || type.IsAbstract)
            throw new MetadataException($"Can't use interfaces or abstract classes: '{type.Name}'");

        type.Name.ValidateInvariantName();
    }

    protected virtual void AddField(IDbEntityMetadata em, PropertyInfo propertyInfo)
    {
        IDbFieldMetadata fm = this.FieldMetadataFactory.CreateType(em, propertyInfo.PropertyType, propertyInfo.Name, null);
        em.AddField(fm);
    }

    protected virtual IDbEntityMetadata CreateMetadata(Type type)
    {
        IDbEntityMetadata em = this.Entities.Get(type.Name); //Daha önce prematüre eklenmiş olabilir
        if (em == null)
        {
            em = this.EntityMetadataFactory.Create(type.Name);
        }
        else
        {
            if (!em.IsPremature)
                throw new MetadataException($"'{type.Name}' already available");
        }

        em.ConcreteTypeName = type.FullName;

        PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

        foreach (PropertyInfo inf in propertyInfos)
        {
            if (!inf.IsUseful())
                continue;

            this.AddField(em, inf);
        }

        if (em.IsPremature)
            em.IsPremature = false;

        return em;
    }

    protected virtual IDbEntityMetadata AddConcreteDefinition(Type type, string module = null, string prefix = null)
    {
        this.Validate();

        Log.Debug("Database", $"Metadata; Add: {type.FullName}");

        try
        {
            this.ValidateConcreteType(type);

            IDbEntityMetadata em = this.CreateMetadata(type);
            em.ModuleName = module;
            em.Prefix = prefix;

            if (em.ModuleName.IsNullOrEmpty())
            {
                //Modülünü bulacağız
                var aInfo = Common.Assembly.FindAssemblyInfo(type.Assembly);
                em.ModuleName = aInfo.NavigationName;

                if (em.Prefix.IsNullOrEmpty())
                    em.Prefix = aInfo.DatabaseEntityPrefix;
            }

            if (em.Prefix.IsNullOrEmpty())
            {
                em.Prefix = DatabaseConstants.PREFIX_DEFAULT;
            }

            this.Add(em);

            return em;
        }
        catch (Exception ex)
        {
            ex.Log();
            throw ex.Translate();
        }
    }

    protected virtual IDbEntityMetadata AddEnumDefinition(Type type, string module = null, string prefix = null)
    {
        this.Validate();

        throw new NotImplementedException();
    }

    public virtual IDbEntityMetadata Add(Type type, string module = null, string prefix = null)
    {
        if (type.IsEnum)
            return this.AddEnumDefinition(type, module, prefix);
        else
            return this.AddConcreteDefinition(type, module, prefix);
    }

}
