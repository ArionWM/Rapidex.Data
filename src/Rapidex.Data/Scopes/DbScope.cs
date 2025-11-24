using Rapidex;
using Rapidex.Data.Entities;
using Rapidex.Data.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data.Scopes
{
    internal class DbScope : IDbScope
    {
#if DEBUG
#pragma warning disable IDE1006 // Naming Styles
        private readonly int _debugTracker;
#pragma warning restore IDE1006 // Naming Styles
#endif

        protected readonly IServiceProvider serviceProvider;
        protected bool isInitialized = false;
        protected ILogger logger;


        protected string name;
        protected string defaultSchemaName;
        protected long id;

        protected IDbSchemaScope baseScope;
        protected IDbProvider dbProvider;

        protected Dictionary<string, IDbSchemaScope> schemaScopes = new Dictionary<string, IDbSchemaScope>(StringComparer.InvariantCultureIgnoreCase);

        public string ConnectionString => dbProvider?.ConnectionString;
        public long Id => id;
        public string Name => name;
        public string DatabaseName => dbProvider?.DatabaseName;
        public string[] Schemas => schemaScopes.Keys.ToArray();

        public string DefaultSchemaName => defaultSchemaName;

        public string SchemaName => baseScope.SchemaName;


        public IDbMetadataContainer Metadata { get; protected set; }

        public IDbScope ParentDbScope => this;
        public IDbProvider DbProvider => dbProvider;

        public IDbStructureProvider Structure => baseScope.Structure;

        public IDbDataModificationStaticHost Data => baseScope.Data;

        public EntityMapper Mapper => baseScope.Mapper;

        public IBlobRepository Blobs => baseScope.Blobs;

        public IDbSchemaScope Base => baseScope;



        public DbScope(IServiceProvider serviceProvider)
        {
#if DEBUG
            this._debugTracker = RandomHelper.Random(1000000);
#endif
            this.serviceProvider = serviceProvider;
            this.logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<DbScope>();
        }

        protected void CheckInitialized()
        {
            if (!this.isInitialized)
                throw new InvalidOperationException($"DbScope not initialized");
        }


        protected IDbSchemaScope AddSchema(string schemaName)
        {
            if (this.schemaScopes.Keys.Contains(schemaName))
                return this.schemaScopes.Get(schemaName);

            DbProviderFactory dbCreator = this.serviceProvider.GetRequiredService<DbProviderFactory>();
            IDbProvider dbProvider = dbCreator.CreateProvider(this.dbProvider.GetType().FullName, this.ConnectionString);

            IDbSchemaScope sscope = this.serviceProvider.GetKeyedService<IDbSchemaScope>("raw").NotNull();
            sscope.Initialize(schemaName, this, dbProvider);

            var validationResults = dbProvider.ValidateConnection();
            if (!validationResults.Success)
                throw new DataConnectionException(validationResults.ToString());

            sscope.Structure.CreateOrUpdateSchema(schemaName);

            schemaScopes.Set(schemaName, sscope);
            sscope.Structure.ApplyAllStructure();

            return sscope;
        }

        protected void LoadRecordedSchemaInfos()
        {
            var siEm = this.Metadata.CheckAndGet<SchemaInfo>();
            siEm.NotNull("SchemaInfo metadata not found");
            this.baseScope.Structure.ApplyEntityStructure(siEm, false);

            var schemas = this.baseScope.Data.Load<SchemaInfo>();
            foreach (var schema in schemas)
            {
                this.AddSchema(schema.Name);
            }
        }

        protected void RecordSchemaInfo(IDbSchemaScope schemaScope, long id = DatabaseConstants.DEFAULT_EMPTY_ID)
        {
            IDbSchemaScope _base = schemaScope.SchemaName == this.DefaultSchemaName ? schemaScope : this.baseScope;

            using var work = _base.BeginWork();

            SchemaInfo schemaRecord = work.New<SchemaInfo>();
            schemaRecord.Name = schemaScope.SchemaName;
            if (id > 0)
                schemaRecord.Id = id;

            schemaRecord.Save();

            work.CommitChanges();
        }

        protected IDbSchemaScope AddSchemaIfNotExistsInternal(string schemaName = null, long id = DatabaseConstants.DEFAULT_EMPTY_ID)
        {
            schemaName.ValidateInvariantName();

            if (this.schemaScopes.ContainsKey(schemaName))
                return this.schemaScopes.Get(schemaName);

            IDbSchemaScope sscope = this.AddSchema(schemaName);
            if (string.Compare(schemaName, this.defaultSchemaName, true) != 0)
                this.RecordSchemaInfo(sscope, id);

            return sscope;
        }

        public IDbSchemaScope AddSchemaIfNotExists(string schemaName = null, long id = DatabaseConstants.DEFAULT_EMPTY_ID)
        {
            this.CheckInitialized();

            return this.AddSchemaIfNotExistsInternal(schemaName, id);
        }

        public IDbSchemaScope Schema(string schemaName = null)
        {
            this.CheckInitialized();

            return this.schemaScopes.Get(schemaName).NotNull($"Schema '{schemaName}' not available");
        }

        public void Setup()
        {

        }

        public void Initialize(long databaseId, string dbName, IDbProvider dbProvider)
        {
            if (this.isInitialized)
                throw new InvalidOperationException($"DbScope already initialized");

            dbName.ValidateInvariantName();

            this.id = databaseId;
            this.name = dbName;

            this.dbProvider = dbProvider;
            this.defaultSchemaName = DatabaseConstants.DEFAULT_SCHEMA_NAME;

            dbProvider.SetParentScope(this);

            this.Metadata = new DbMetadataContainer(this);

            //Call all assemblies to setup metadata
            Common.Assembly.IterateAsemblies(ass =>
            {
                if (ass is IRapidexMetadataReleatedAssemblyDefinition mass)
                    mass.SetupMetadata(this);
            });


            //Check 'base' schema
            this.baseScope = this.AddSchemaIfNotExistsInternal(this.DefaultSchemaName, 1);
            this.baseScope.Structure.ApplyAllStructure();

            //Load other schemas
            this.LoadRecordedSchemaInfos();

            this.isInitialized = true;
        }

        public IIntSequence Sequence(string name)
        {
            return baseScope.Data.Sequence(name);
        }


    }
}
