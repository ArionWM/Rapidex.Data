using Rapidex.Data.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data.Scopes
{
    internal class DbSchemaScope : IDbSchemaScope
    {
        protected readonly IDbScope parentDbScope;
        protected readonly IDbProvider dbProvider;
        protected readonly string name;
        protected IDbStructureProvider structureManager;
        protected IDbDataModificationManager dataManager;
        protected EntityMapper mapper;
        protected IBlobRepository blobRepository;

        public IDbScope ParentDbScope => parentDbScope;

        public IDbProvider DbProvider => dbProvider;

        public string SchemaName => name;

        public IDbStructureProvider Structure => structureManager;

        public IDbDataModificationManager Data => dataManager;

        public EntityMapper Mapper => mapper;

        public IBlobRepository Blobs => blobRepository;

        public bool IsTransactionAvailable => dataManager.IsTransactionAvailable;

        public DbSchemaScope(string schemaName, IDbScope parentDbScope, IDbProvider provider)
        {
            this.name = schemaName;
            this.parentDbScope = parentDbScope;

            this.dbProvider = provider;
            provider.SetParentScope(this);
            this.structureManager = this.dbProvider.GetStructureProvider();

            var dmProvider = this.dbProvider.GetDataModificationPovider();
            this.dataManager = new DbDataModificationManager(this, dmProvider);
            //this.metadataManager = new DbEntityMetadataManager(this);
            this.mapper = new EntityMapper(this);
            this.blobRepository = new DefaultDbBlobRepository(this);

        }

        public virtual void Setup()
        {
            this.dbProvider.Setup(null); //?Services bu aşamada verilemez ki?
            this.dbProvider.Start();
        }

        public IIntSequence Sequence(string name)
        {
            return dataManager.Sequence(name);
        }



    }
}
