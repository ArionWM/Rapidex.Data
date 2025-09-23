using Rapidex.Data.DataModification;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data
{
    public static class DataModificationExtensions
    {


        /// <summary>
        /// Drop table or collection on database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        public static void DropEntity<T>(this IDbStructureProvider provider) where T : IConcreteEntity
        {
            provider.DropEntity(typeof(T).Name);

        }

        public static void DropEntity(this IDbStructureProvider provider, string entityName)
        {
            entityName.NotEmpty();
            var em = provider.ParentScope.ParentDbScope.Metadata.Get(entityName)
                .NotNull($"Entity {entityName} not found in metadata");

            provider.DropEntity(em);
        }

        public static void ApplyEntityStructure<T>(this IDbStructureProvider provider, bool applyScopedData = false) where T : IConcreteEntity
        {
            var em = provider.ParentScope.ParentDbScope.Metadata.Get<T>().NotNull();
            provider.ApplyEntityStructure(em, applyScopedData);
        }

        public static Task<bool> IsDatabaseAvailableAsync(this IDbStructureProvider provider, string dbName)
        {
            return Task.Run(() => provider.IsDatabaseAvailable(dbName));
        }

        public static Task<bool> IsSchemaAvailableAsync(this IDbStructureProvider provider, string schemaName)
        {
            return Task.Run(() => provider.IsSchemaAvailable(schemaName));
        }

        public static Task<bool> IsExistsAsync(this IDbStructureProvider provider, string schemaName, string entityName)
        {
            return Task.Run(() => provider.IsExists(schemaName, entityName));
        }

        public static Task<bool> IsExistsAsync(this IDbStructureProvider provider, string schemaName, string entityName, IDbFieldMetadata cm)
        {
            return Task.Run(() => provider.IsExists(schemaName, entityName, cm));
        }

        public static Task CreateDatabaseAsync(this IDbStructureProvider provider, string dbName)
        {
            return Task.Run(() => provider.CreateDatabase(dbName));
        }

        public static Task DestroyDatabaseAsync(this IDbStructureProvider provider, string dbName)
        {
            return Task.Run(() => provider.DestroyDatabase(dbName));
        }

        public static Task SwitchDatabaseAsync(this IDbStructureProvider provider, string dbName)
        {
            return Task.Run(() => provider.SwitchDatabase(dbName));
        }

        public static Task CreateOrUpdateSchemaAsync(this IDbStructureProvider provider, string schemaName)
        {
            return Task.Run(() => provider.CreateOrUpdateSchema(schemaName));
        }

        public static Task DestroySchemaAsync(this IDbStructureProvider provider, string schemaName)
        {
            return Task.Run(() => provider.DestroySchema(schemaName));
        }

        public static Task ApplyEntityStructureAsync(this IDbStructureProvider provider, IDbEntityMetadata entityMetadata, bool applyScopedData = false)
        {
            return Task.Run(() => provider.ApplyEntityStructure(entityMetadata, applyScopedData));
        }

        public static Task DropEntityAsync(this IDbStructureProvider provider, IDbEntityMetadata entityMetadata)
        {
            return Task.Run(() => provider.DropEntity(entityMetadata));
        }

        public static Task ApplyAllStructureAsync(this IDbStructureProvider provider)
        {
            return Task.Run(() => provider.ApplyAllStructure());
        }

        public static Task CreateSequenceIfNotExistsAsync(this IDbStructureProvider provider, string name, int minValue = -1, int startValue = -1)
        {
            return Task.Run(() => provider.CreateSequenceIfNotExists(name, minValue, startValue));
        }
    }
}
