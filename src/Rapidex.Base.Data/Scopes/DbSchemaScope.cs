using Rapidex.Data.DataModification;
using Rapidex.Data.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data.Scopes
{
    internal class DbSchemaScope : IDbSchemaScope
    {
        private readonly int _debugTracker;
        protected readonly IDbScope parentDbScope;
        protected readonly IDbProvider dbProvider;
        protected readonly string name;
        protected IDbStructureProvider structureManager;
        protected IDbDataModificationStaticHost dataManager;
        protected EntityMapper mapper;
        protected IBlobRepository blobRepository;

        public IDbScope ParentDbScope => parentDbScope;

        public IDbProvider DbProvider => dbProvider;

        public string SchemaName => name;

        public IDbStructureProvider Structure => structureManager;

        public IDbDataModificationStaticHost Data => dataManager;

        public EntityMapper Mapper => mapper;

        public IBlobRepository Blobs => blobRepository;


        public DbSchemaScope(string schemaName, IDbScope parentDbScope, IDbProvider provider)
        {
            this.name = schemaName;
            this.parentDbScope = parentDbScope;

            this.dbProvider = provider;
            provider.SetParentScope(this);
            this.structureManager = this.dbProvider.GetStructureProvider();

            this.dataManager = new DataModificationStaticHost(this);
            this.mapper = new EntityMapper(this);
            this.blobRepository = new DefaultDbBlobRepository(this);
            this._debugTracker = RandomHelper.Random(1000000);
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
