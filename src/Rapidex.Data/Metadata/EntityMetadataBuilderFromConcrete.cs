using Rapidex.Base.Common.Assemblies;
using Rapidex.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.Metadata;
internal class EntityMetadataBuilderFromConcrete : EntityMetadataBuilderBase
{
    public EntityMetadataBuilderFromEnum EnumerationDefinitionFactory { get; }

    public EntityMetadataBuilderFromConcrete(IDbMetadataContainer parent, IDbEntityMetadataFactory dbEntityMetadataFactory, IFieldMetadataFactory fieldMetadataFactory) : base(parent, dbEntityMetadataFactory, fieldMetadataFactory)
    {
        this.EnumerationDefinitionFactory = new EntityMetadataBuilderFromEnum(parent, dbEntityMetadataFactory, fieldMetadataFactory);
    }

    protected virtual void ValidateConcreteType(Type type)
    {
        if (type.IsInterface || type.IsAbstract)
            throw new MetadataException($"Can't use interfaces or abstract classes: '{type.Name}'");

        type.Name.ValidateInvariantName();
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
                    //Common.DefaultLogger?.LogDebug(string.Format("Enumeration field: {0} / {1} / {}", em.Name, field.Name, enumType.Name));

                    this.EnumerationDefinitionFactory.Add(enumType);
                }
            }
        }
    }

    protected virtual void AddField(IDbEntityMetadata em, PropertyInfo propertyInfo)
    {
        IDbFieldMetadata fm = this.FieldMetadataFactory.Create(em, propertyInfo.PropertyType, propertyInfo.Name, null);
        em.AddField(fm);
    }

    public override void Check(IDbEntityMetadata em)
    {
        base.Check(em);
        this.CheckEnumerations(em);
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
        em.Parent = this.Parent;

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

    protected virtual void CheckAndApplyConcreteImplementer(Type type, IDbEntityMetadata em)
    {
        Type intType = typeof(IConcreteEntityImplementer<>);
        Type intGenericType = intType.MakeGenericType(type);
        Type[] types = Common.Assembly.FindDerivedClassTypes(intGenericType);

        types = TypeInheritenceHelper.SortTypesByInheritanceHierarchy(types);

        IEnumerable<Type> endTypes = types.RemoveBaseTypes();
        if(endTypes.IsNullOrEmpty())
            return;

        foreach(Type implementerType in endTypes)
        {
            IConcreteEntityImplementer implementer = TypeHelper.CreateInstance<IConcreteEntityImplementer>(implementerType);
            implementer.SetupMetadata(this.Parent.DbScope, em);
        }
    }

    protected virtual IDbEntityMetadata AddConcreteDefinition(Type type, string module = null, string prefix = null)
    {
        this.Validate();

        Common.DefaultLogger?.LogDebug( $"Metadata; Add: {type.FullName}");

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

        this.CheckAndApplyConcreteImplementer(type, em);

        this.Add(em);

        return em;

    }


    public virtual IDbEntityMetadata Add(Type type, string module = null, string prefix = null)
    {
        IDbEntityMetadata em;
        if (type.IsEnum)
            em = this.EnumerationDefinitionFactory.Add(type, module, prefix);
        else
            em = this.AddConcreteDefinition(type, module, prefix);

        em.Parent = this.Parent;
        return em;
    }

}
