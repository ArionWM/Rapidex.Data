using Rapidex.Base.Common.Assemblies;
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
        throw new NotImplementedException();
    }

    public static IDbEntityMetadata GetMetadata(this IEntity ent)
    {
        ent.NotNull();

        IDbEntityMetadata em = ent._Metadata ?? ent._Scope.ParentDbScope.Metadata.Get(ent._TypeName);
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
        var em = mContainer.Get<T>();
        if (em == null || em.IsPremature)
        {
            mContainer.Add<T>(module, prefix);

        }
        return em;
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





    
    public static void Remove(this IDbMetadataContainer emman, string name)
    {
        IDbEntityMetadata em = emman.Get(name);
        if (em != null)
        {
            emman.Remove(em.Name);
        }
    }

    
    public static void Remove<T>(this IDbMetadataContainer emman) where T : IConcreteEntity
    {
        emman.Remove(typeof(T).Name);
    }

    //public static IDbEntityMetadata UpdateOwner(this IDbEntityMetadata em, IRapidexAssemblyDefinition module)
    //{
    //    em.ModuleName = module.NavigationName;
    //    em.Prefix = module.DatabaseEntityPrefix;

    //    return em;
    //}

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

    public static IDbEntityMetadata MarkShowInPreview(this IDbEntityMetadata em) //TODO: Ui level'a taşınacak ..
    {
        em.Tags.Add("ShowInPreview");
        return em;
    }

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






    
    public static DbFieldMetadataList AddIfNotExist(this DbFieldMetadataList fields, IFieldMetadataFactory fieldMetadataFactory, string name, Type type, string caption, Action<IDbFieldMetadata> set = null)
    {

        IDbFieldMetadata fm = fieldMetadataFactory.CreateType(fields.EntityMetadata, type, name, null);
        fields.AddIfNotExist(fm);
        return fields;
    }

    
    public static DbFieldMetadataList AddfNotExist<T>(this DbFieldMetadataList fields, IFieldMetadataFactory fieldMetadataFactory, string name, string caption, Action<IDbFieldMetadata> set = null) //where T : IDataType
    {
        fields.AddIfNotExist(fieldMetadataFactory, name, typeof(T), caption, set);
        return fields;
    }

    [Obsolete("Use DbScope.Metadata instead", true)]
    public static DbFieldMetadataList AddfNotExist(this DbFieldMetadataList fields, IFieldMetadataFactory fieldMetadataFactory, string name, string type, string caption, ObjDictionary values = null, Action<IDbFieldMetadata> set = null)
    {


        IDbFieldMetadata fm = fieldMetadataFactory.CreateType(fields.EntityMetadata, type, name, values);
        if (caption.IsNOTNullOrEmpty())
        {
            fm.Caption = caption;
        }

        fields.AddIfNotExist(fm);
        return fields;
    }


    public static IDbFieldMetadata AddFieldIfNotExist(this IDbEntityMetadata em, IFieldMetadataFactory fieldMetadataFactory, string name, string type, ObjDictionary values = null)
    {
        IDbFieldMetadata fm = fieldMetadataFactory.CreateType(em, type, name, values);
        em.AddFieldIfNotExist(fm);
        return fm;
    }

    public static IDbFieldMetadata AddFieldIfNotExist<T>(this IDbEntityMetadata em, IFieldMetadataFactory fieldMetadataFactory, string name, ObjDictionary values = null)
    {
        IDbFieldMetadata fm = fieldMetadataFactory.CreateType(em, typeof(T), name, values);
        em.AddFieldIfNotExist(fm);
        return fm;
    }


    public static IDbFieldMetadata AddFieldIfNotExist(this IDbEntityMetadata em, IFieldMetadataFactory fieldMetadataFactory, string name, Type type, string caption, Action<IDbFieldMetadata> set = null)
    {
        IDbFieldMetadata fm = fieldMetadataFactory.CreateType(em, type, name, null);
        set?.Invoke(fm);
        em.Fields.AddIfNotExist(fm);
        return fm;
    }

}
