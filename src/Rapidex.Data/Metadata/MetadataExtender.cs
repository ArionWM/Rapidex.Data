using Rapidex.Base.Common.Assemblies;
using Rapidex.Data.Metadata;
using Rapidex.Data.Metadata.Implementers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rapidex.Data;

public static class MetadataExtender
{
    public static IDbEntityMetadata Get<T>(this IDbMetadataContainer mman) where T : IConcreteEntity
    {
        Type type = typeof(T);
        return mman.Get(type.Name);
    }

    public static M Get<T, M>(this IDbMetadataContainer mman)
        where T : IConcreteEntity
        where M : IDbEntityMetadata
    {
        Type type = typeof(T);
        return (M)mman.Get(type.Name);
    }



    public static IDbEntityMetadata AddPremature(this IDbMetadataContainer mman, string entityName)
    {
        if (mman.Get(entityName) != null)
        {
            throw new InvalidOperationException($"Entity metadata '{entityName}' already exists in the metadata container.");
        }

        var em = Database.EntityMetadataFactory.Create(entityName);
        em.IsPremature = true;
        mman.Add(em);
        return em;
    }

    public static IDbEntityMetadata GetMetadata(this IEntity ent)
    {
        ent.NotNull();

        IDbEntityMetadata em = ent._Metadata ?? ent._Schema.ParentDbScope.Metadata.Get(ent._TypeName);
        //Database.Metadata.Get(ent._TypeName);
        return em;
    }

    public static IDbEntityMetadata AddIfNotExist(this IDbMetadataContainer mContainer, IDbEntityMetadata em)
    {
        var _em = mContainer.Get(em.Name);
        if (_em == null || _em.IsPremature)
        {
            mContainer.Add(em);

        }
        return em;
    }

    public static IDbEntityMetadata AddIfNotExist<T>(this IDbMetadataContainer mContainer, string module = null, string prefix = null) where T : IConcreteEntity
    {
        var _em = mContainer.Get<T>();
        if (_em == null || _em.IsPremature)
        {
            _em = mContainer.Add<T>(module, prefix);

        }
        return _em;
    }


    public static M AddIfNotExist<T, M>(this IDbMetadataContainer mman)
       where T : IConcreteEntity
       where M : IDbEntityMetadata
    {
        Type type = typeof(T);
        return (M)mman.AddIfNotExist<T>();
    }

    public static IDbEntityMetadata CheckAndGet<T>(this IDbMetadataContainer mman) where T : IConcreteEntity
    {
        return mman.AddIfNotExist<T>();
    }

    public static void Remove<T>(this IDbMetadataContainer emman) where T : IConcreteEntity
    {
        if (emman.Get<T>() != null)
            emman.Remove(typeof(T).Name);
    }

    //TODO: Move to application level libraries
    public static IDbEntityMetadata MarkShowInSettings(this IDbEntityMetadata em, bool onlyDeveloperMode) //TODO: Ui level'a taşınacak ..
    {
        if (onlyDeveloperMode)
        {
            em.Tags.Add("ShowInSettingsDM");
        }
        else
        {
            em.Tags.Add("ShowInSettings");
        }

        return em;
    }

    //TODO: Move to application level libraries
    public static IDbEntityMetadata MarkShowInPreview(this IDbEntityMetadata em) //TODO: Ui level'a taşınacak ..
    {
        em.Tags.Add("ShowInPreview");
        return em;
    }

    //TODO: Move to application level libraries
    public static IDbEntityMetadata MarkShowAllFields(this IDbEntityMetadata em) //TODO: Ui level'a taşınacak ..
    {
        em.Tags.Add("ShowAllFields");
        return em;
    }

    public static IDbEntityMetadata MarkOnlyBaseSchema(this IDbEntityMetadata em) //TODO: Ui level'a taşınacak ..
    {
        em.OnlyBaseSchema = true;
        return em;
    }


    public static IDbEntityMetadata[] GetModuleOwnedDefinitions(this IDbMetadataContainer emman, string moduleName)
    {
        var ems = emman.GetAll().Where(em => string.Equals(em.ModuleName, moduleName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
        return ems;
    }


    public static IDbEntityMetadata[] GetModuleOwnedDefinitions(this IDbMetadataContainer emman, IRapidexAssemblyDefinition module)
    {
        return emman.GetModuleOwnedDefinitions(module.NavigationName);
    }

    public static IFieldMetadataFactory GetFieldMetadataFactory(this IDbMetadataContainer container)
    {
        container.NotNull();
        FieldMetadataFactory fmf = new(container);
        return fmf;
    }

    public static DbFieldMetadataList AddIfNotExist(this DbFieldMetadataList fields, string name, Type type, string caption, Action<IDbFieldMetadata> set = null)
    {

        IDbFieldMetadata fm = fields.EntityMetadata.Parent.GetFieldMetadataFactory().Create(fields.EntityMetadata, type, name, null);
        fields.AddIfNotExist(fm);
        return fields;
    }


    public static DbFieldMetadataList AddIfNotExist<T>(this DbFieldMetadataList fields, string name, string caption, Action<IDbFieldMetadata> set = null) //where T : IDataType
    {
        fields.AddIfNotExist(name, typeof(T), caption, set);
        return fields;
    }

    public static DbFieldMetadataList AddIfNotExist(this DbFieldMetadataList fields, string name, string type, string caption, ObjDictionary values = null, Action<IDbFieldMetadata> set = null)
    {
        IDbFieldMetadata fm = fields.EntityMetadata.Parent.GetFieldMetadataFactory().Create(fields.EntityMetadata, type, name, values);
        if (caption.IsNOTNullOrEmpty())
        {
            fm.Caption = caption;
        }

        fields.AddIfNotExist(fm);
        return fields;
    }

    public static IDbFieldMetadata AddFieldIfNotExist(this IDbEntityMetadata em, IDbFieldMetadata fm)
    {
        em.Fields.AddIfNotExist(fm);
        return fm;
    }


    public static IDbFieldMetadata AddFieldIfNotExist(this IDbEntityMetadata em, string name, string type, ObjDictionary values = null)
    {
        IDbFieldMetadata fm = em.Parent.GetFieldMetadataFactory().Create(em, type, name, values);
        em.AddFieldIfNotExist(fm);
        return fm;
    }

    public static IDbFieldMetadata AddFieldIfNotExist<T>(this IDbEntityMetadata em, string name, ObjDictionary values = null)
    {
        IDbFieldMetadata fm = em.Parent.GetFieldMetadataFactory().Create(em, typeof(T), name, values);
        em.AddFieldIfNotExist(fm);
        return fm;
    }


    public static IDbFieldMetadata AddFieldIfNotExist(this IDbEntityMetadata em, string name, Type type, string caption, Action<IDbFieldMetadata> set = null)
    {
        IDbFieldMetadata fm = em.Parent.GetFieldMetadataFactory().Create(em, type, name, null);
        set?.Invoke(fm);
        em.Fields.AddIfNotExist(fm);
        return fm;
    }

    internal static void Check(this IDbEntityMetadata em)
    {
        EntityMetadataBuilderFromConcrete mb = new(em.Parent, Database.EntityMetadataFactory, em.Parent.GetFieldMetadataFactory());
        mb.Check(em);
    }
}
