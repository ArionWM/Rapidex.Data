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

        protected readonly string name;
        protected readonly string defaultSchemaName;
        protected readonly long id;

        protected IDbSchemaScope baseScope;
        protected readonly IDbProvider dbProvider;

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

        public IDbDataModificationManager Data => baseScope.Data;

        public EntityMapper Mapper => baseScope.Mapper;

        public IBlobRepository Blobs => baseScope.Blobs;

        public IDbSchemaScope Base => baseScope;


        public DbScope(long id, string name, IDbProvider dbProvider)
        {
            name.ValidateInvariantName();

            this.id = id;
            this.name = name;
            //this.databaseName = databaseName;
            this.dbProvider = dbProvider;
            this.defaultSchemaName = DatabaseConstants.DEFAULT_SCHEMA_NAME;

            dbProvider.SetParentScope(this);

            this.Initialize();
        }

        protected void Initialize()
        {
            this.Metadata = new DbMetadataContainer(this);

            var structureManager = this.DbProvider.GetStructureProvider();
            if (!structureManager.IsDatabaseAvailable(this.DatabaseName))
            {
                structureManager.CreateDatabase(this.DatabaseName);
            }

            this.DbProvider.UseDb(this.DatabaseName);

            this.baseScope = this.AddSchemaIfNotExists(this.DefaultSchemaName, 1);
            this.baseScope.Structure.ApplyAllStructure();

            this.LoadRecordedSchemaInfos();
        }

        protected DbSchemaScope AddSchema(string schemaName)
        {
            if (this.schemaScopes.Keys.Contains(schemaName))
                this.schemaScopes.Get(schemaName);

            DbProviderFactory dbCreator = new DbProviderFactory();
            IDbProvider dbProvider = dbCreator.CreateProvider(this.dbProvider.GetType().FullName, this.ConnectionString);

            DbSchemaScope sscope = new DbSchemaScope(schemaName, this, dbProvider);

            var validationResults = dbProvider.ValidateConnection();
            if (!validationResults.Success)
                throw new DataConnectionException(validationResults.ToString());


            sscope.Structure.CreateOrUpdateSchema(schemaName);
            sscope.Setup();
            schemaScopes.Set(schemaName, sscope);
            sscope.Structure.ApplyAllStructure();

            return sscope;
        }

        protected void LoadRecordedSchemaInfos()
        {
            var siEm = this.Metadata.CheckAndGet<SchemaInfo>();
            siEm.NotNull("SchemaInfo metadata not found");
            this.baseScope.Structure.ApplyEntityStructure(siEm, false);

            var schemas = this.baseScope.Data.Load<SchemaInfo>().Result;
            foreach (var schema in schemas)
            {
                AddSchema(schema.Name);
            }
        }

        protected void RecordSchemaInfo(IDbSchemaScope schemaScope, long id = DatabaseConstants.DEFAULT_EMPTY_ID)
        {
            IDbSchemaScope _base = schemaScope.SchemaName == this.DefaultSchemaName ? schemaScope : this.baseScope;

            SchemaInfo schemaRecord = _base.New<SchemaInfo>();
            schemaRecord.Name = schemaScope.SchemaName;
            if (id > 0)
                schemaRecord.Id = id;

            schemaRecord.Save();

            _base.CommitOrApplyChanges()
                .Wait();
        }

        public IDbSchemaScope AddSchemaIfNotExists(string schemaName = null, long id = DatabaseConstants.DEFAULT_EMPTY_ID)
        {
            schemaName.ValidateInvariantName();

            if (this.schemaScopes.ContainsKey(schemaName))
                return this.Schema(schemaName);

            DbSchemaScope sscope = this.AddSchema(schemaName);
            if (string.Compare(schemaName, this.defaultSchemaName, true) != 0)
                this.RecordSchemaInfo(sscope, id);

            return sscope;
        }

        public IDbSchemaScope Schema(string schemaName = null)
        {
            return this.schemaScopes.Get(schemaName).NotNull($"Schema '{schemaName}' not available");
        }

        public void Setup()
        {

        }

        public IIntSequence Sequence(string name)
        {
            return baseScope.Data.Sequence(name);
        }


    }
}
