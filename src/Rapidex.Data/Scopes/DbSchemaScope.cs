using Rapidex.Data.DataModification;
using Rapidex.Data.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data.Scopes
{
    internal class DbSchemaScope : IDbSchemaScope
    {
#if DEBUG
#pragma warning disable IDE1006 // Naming Styles
        private readonly int _debugTracker;
#pragma warning restore IDE1006 // Naming Styles
#endif
        protected bool isInitialized = false;
        protected ILogger logger;
        protected IDbScope parentDbScope;
        protected IDbProvider dbProvider;
        protected string name;
        protected IDbStructureProvider structureManager;
        protected IDbDataModificationStaticHost dataManager;
        protected EntityMapper mapper;
        protected IBlobRepository blobRepository;

        public IDbScope ParentDbScope
        {
            get
            {
                //this.CheckInitialized();
                return parentDbScope;
            }
        }

        public IDbProvider DbProvider
        {
            get
            {
                //this.CheckInitialized();
                return dbProvider;
            }
        }

        public string SchemaName => name;

        public IDbStructureProvider Structure
        {
            get
            {
                this.CheckInitialized();
                return this.structureManager;
            }
        }

        public IDbDataModificationStaticHost Data
        {
            get
            {
                this.CheckInitialized();
                return this.dataManager;
            }
        }

        public EntityMapper Mapper
        {
            get
            {
                this.CheckInitialized();
                return this.mapper;
            }
        }

        public IBlobRepository Blobs => blobRepository;


        public DbSchemaScope(IServiceProvider serviceProvider)
        {
#if DEBUG
            this._debugTracker = RandomHelper.Random(1000000);
#endif

            this.logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<DbSchemaScope>();

        }

        protected void CheckInitialized()
        {
            if (!this.isInitialized)
                throw new InvalidOperationException("Scope is not initialized.");
        }

        public virtual void Initialize(string schemaName, IDbScope parentDbScope, IDbProvider provider)
        {
            if (this.isInitialized)
                throw new InvalidOperationException("Scope is already initialized.");

            this.name = schemaName;
            this.parentDbScope = parentDbScope;
            this.dbProvider = provider;
            provider.SetParentScope(this);
            this.structureManager = this.dbProvider.GetStructureProvider();

            this.dataManager = new DataModificationStaticHost(this);
            this.mapper = new EntityMapper(this);
            this.blobRepository = new DefaultDbBlobRepository(this);
            this.isInitialized = true;

        }

        public IIntSequence Sequence(string name)
        {
            this.CheckInitialized();
            return dataManager.Sequence(name);
        }



    }
}
