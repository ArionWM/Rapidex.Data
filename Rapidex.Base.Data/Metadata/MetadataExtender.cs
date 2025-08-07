using Rapidex.Data.Metadata;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rapidex.Data
{
    public static class MetadataExtender
    {
        public static IDbEntityMetadata Get<T>(this IDbEntityMetadataManager mman) where T : IConcreteEntity
        {
            Type type = typeof(T);
            return mman.Get(type.Name);
        }

        public static M Get<T, M>(this IDbEntityMetadataManager mman)
            where T : IConcreteEntity
            where M : IDbEntityMetadata
        {
            Type type = typeof(T);
            return (M)mman.Get(type.Name);
        }

        public static IDbEntityMetadata GetMetadata(this IEntity ent)
        {
            ent.NotNull();

            IDbEntityMetadata em = ent._Metadata ?? Database.Metadata.Get(ent._TypeName);
            return em;
        }
        public static IDbEntityMetadata AddIfNotExist(this IDbEntityMetadataManager emman, Type concreteType, string module = null, string prefix = null)
        {
            emman.NotNull();

            IDbEntityMetadata em = emman.Get(concreteType.Name);
            if (em == null)
            {
                em = emman.Add(concreteType, module, prefix);
            }
            else
            {
                if (em != null && em.IsPremature)
                {
                    em = emman.Add(concreteType, module, prefix);
                }
            }

            return em;
        }

        public static IDbEntityMetadata AddIfNotExist<T>(this IDbEntityMetadataManager emman, string module = null, string prefix = null) where T : IConcreteEntity
        {
            emman.NotNull();

            IDbEntityMetadata em = emman.Get<T>();
            if (em == null)
            {
                em = emman.Add<T>(module, prefix);
            }
            else
            {
                if (em != null && em.IsPremature)
                {
                    em = emman.Add<T>(module, prefix);
                }
            }

            return em;
        }

        internal static IDbEntityMetadata CheckAndGet<T>(this IDbEntityMetadataManager mman) where T : IConcreteEntity
        {
            return mman.AddIfNotExist<T>();
        }

        public static void AddIfNotExist(this IDbEntityMetadataManager emman, IDbEntityMetadata em)
        {
            emman.NotNull();

            IDbEntityMetadata _em = emman.Get(em.Name);
            if (_em == null || _em.IsPremature)
            {
                emman.Add(em);
            }
        }

        public static M AddIfNotExist<T, M>(this IDbEntityMetadataManager mman)
           where T : IConcreteEntity
           where M : IDbEntityMetadata
        {
            Type type = typeof(T);
            return (M)mman.AddIfNotExist<T>();
        }

        public static DbFieldMetadataList AddfNotExist(this DbFieldMetadataList fields, string name, Type type, string caption, Action<IDbFieldMetadata> set = null)
        {

            IDbFieldMetadata fm = Database.Metadata.FieldMetadataFactory.CreateType(fields.EntityMetadata, type, name, null);
            fields.AddIfNotExist(fm);
            return fields;
        }

        public static DbFieldMetadataList AddfNotExist<T>(this DbFieldMetadataList fields, string name, string caption, Action<IDbFieldMetadata> set = null) //where T : IDataType
        {
            fields.AddfNotExist(name, typeof(T), caption, set);
            return fields;
        }

        public static DbFieldMetadataList AddfNotExist(this DbFieldMetadataList fields, string name, string type, string caption, ObjDictionary values = null, Action<IDbFieldMetadata> set = null)
        {


            IDbFieldMetadata fm = Database.Metadata.FieldMetadataFactory.CreateType(fields.EntityMetadata, type, name, values);
            if (caption.IsNOTNullOrEmpty())
            {
                fm.Caption = caption;
            }

            fields.AddIfNotExist(fm);
            return fields;
        }


        public static IDbFieldMetadata AddFieldIfNotExist(this IDbEntityMetadata em, string name, string type, ObjDictionary values = null)
        {
            //IDbSchemaScope scope = Database.Scopes.Db(em.DbName).Schema(em.SchemaName);

            IDbFieldMetadata fm = Database.Metadata.FieldMetadataFactory.CreateType(em, type, name, values);
            em.AddFieldIfNotExist(fm);
            return fm;
        }

        public static IDbFieldMetadata AddFieldIfNotExist<T>(this IDbEntityMetadata em, string name, ObjDictionary values = null)
        {
            //IDbSchemaScope scope = Database.Scopes.Db(em.DbName).Schema(em.SchemaName);

            IDbFieldMetadata fm = Database.Metadata.FieldMetadataFactory.CreateType(em, typeof(T), name, values);
            em.AddFieldIfNotExist(fm);
            return fm;
        }

        public static IDbFieldMetadata AddFieldIfNotExist(this IDbEntityMetadata em, string name, Type type, string caption, Action<IDbFieldMetadata> set = null)
        {
            IDbFieldMetadata fm = Database.Metadata.FieldMetadataFactory.CreateType(em, type, name, null);
            set?.Invoke(fm);
            em.Fields.AddIfNotExist(fm);
            return fm;
        }

        public static void Remove(this IDbEntityMetadataManager emman, string name)
        {
            IDbEntityMetadata em = emman.Get(name);
            if (em != null)
            {
                emman.Remove(em);
            }
        }

        public static void Remove<T>(this IDbEntityMetadataManager emman) where T : IConcreteEntity
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


        [Obsolete("Use ModuleDefinition.Entities")]
        public static IDbEntityMetadata[] GetModuleOwnedDefinitions(this IDbEntityMetadataManager emman, string moduleName)
        {
            var ems = emman.GetAll().Where(em => string.Equals(em.ModuleName, moduleName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            return ems;
        }

        [Obsolete("Use ModuleDefinition.Entities")]

        public static IDbEntityMetadata[] GetModuleOwnedDefinitions(this IDbEntityMetadataManager emman, IRapidexAssemblyDefinition module)
        {
            return emman.GetModuleOwnedDefinitions(module.NavigationName);
        }



    }
}
