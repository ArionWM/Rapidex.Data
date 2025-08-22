using Npgsql;
using NpgsqlTypes;
using Rapidex.Data;
using Rapidex.Data.PostgreServer;
using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using static Rapidex.Data.RelationN2N;

namespace Rapidex.Data.PostgreServer;

public class PostgreSqlStructureProvider(IDbProvider parent, string connectionString) : IDbStructureProvider
{
    protected NpgsqlConnectionStringBuilder Connectionbuilder { get; set; }
    protected PostgreSqlDdlGenerator DdlGenerator { get; set; } = new PostgreSqlDdlGenerator();
    internal PostgreSqlServerConnection Connection { get; set; }

    public string ConnectionString => connectionString;
    public IDbProvider ParentDbProvider => parent;

    public IDbSchemaScope ParentScope => parent.ParentScope;

    protected bool CheckConnection(bool tryMaster = false)
    {
        try
        {
            if (this.Connectionbuilder == null)
                this.Connectionbuilder = new NpgsqlConnectionStringBuilder(this.ConnectionString);

            if (this.Connection == null)
                this.Connection = new PostgreSqlServerConnection(((IDbStructureProvider)this).ConnectionString);

            return true;
        }
        catch (PostgresException pex)
        {
            var tex = PostgreSqlServerProvider.PostgreServerExceptionTranslator.Translate(pex);
            tex.Log();
            throw tex;
        }
        catch (Exception ex)
        {
            var tex = PostgreSqlServerProvider.PostgreServerExceptionTranslator.Translate(ex) ?? ex;
            tex.Log();
            throw tex;
        }
    }

    protected bool CanApplyToSchema(IDbEntityMetadata em)
    {
        IDbSchemaScope scope = this.ParentDbProvider.ParentScope.NotNull("Parent scope is null");
        bool canApply = !em.OnlyBaseSchema || scope.SchemaName == DatabaseConstants.DEFAULT_SCHEMA_NAME;
        return canApply;
    }

    public void SwitchDatabase(string dbName)
    {
        // In PostgreSQL, you must reconnect with a new connection string to switch databases.
        // This method is a no-op or can be used to re-instantiate the connection.
        this.Connection = new PostgreSqlServerConnection(
            new NpgsqlConnectionStringBuilder(this.ConnectionString) { Database = dbName }.ToString()
        );
    }

    public bool IsDatabaseAvailable(string dbName)
    {
        bool directTargetDatabase = this.CheckConnection(true);
        string sql = this.DdlGenerator.IsDatabaseAvailable(dbName);
        DataTable dataTable = this.Connection.Execute(sql);
        bool isAvail = dataTable.Rows.Count > 0;
        return isAvail;
    }

    public bool IsExists(string schemaName, string entityName)
    {
        schemaName = PostgreHelper.CheckObjectName(schemaName);
        entityName = PostgreHelper.CheckObjectName(entityName);
        this.CheckConnection();
        string sql = this.DdlGenerator.IsTableAvailable(schemaName, entityName);
        DataTable dataTable = this.Connection.Execute(sql);
        bool isAvail = dataTable.Rows.Count > 0;
        return isAvail;
    }

    public bool IsExists(string schemaName, string entityName, IDbFieldMetadata cm)
    {
        schemaName = PostgreHelper.CheckObjectName(schemaName);
        entityName = PostgreHelper.CheckObjectName(entityName);
        this.CheckConnection();
        string sql = this.DdlGenerator.IsColumnAvailable(schemaName, entityName, cm.Name);
        DataTable dataTable = this.Connection.Execute(sql);
        bool isAvail = dataTable.Rows.Count > 0;
        return isAvail;
    }

    public bool IsSchemaAvailable(string schemaName)
    {
        schemaName = PostgreHelper.CheckObjectName(schemaName);

        this.CheckConnection();
        string sql = this.DdlGenerator.IsSchemaAvailable(schemaName);
        DataTable dataTable = this.Connection.Execute(sql);
        bool isAvail = dataTable.Rows.Count > 0;
        return isAvail;
    }

    public void CreateDatabase(string dbName)
    {
        dbName.NotEmpty();

        this.CheckConnection();

        try
        {
            string sql1 = this.DdlGenerator.CreateDatabase01(dbName, this.Connectionbuilder.Username);
            this.Connection.Execute(sql1);

            // In PostgreSQL, owner is set at creation, so CreateDatabase02 is not needed.
            // this.DdlGenerator.CreateDatabase02(dbName, this.Connectionbuilder.Username);

            this.SwitchDatabase(dbName);
        }
        catch (PostgresException pex) when (pex.SqlState == "42P04") // duplicate_database
        {
            throw pex;
        }
        catch (PostgresException pex) when (pex.SqlState == "42501") // insufficient_privilege
        {
            pex.Log();
            throw new Exception($"Db Permission denied: {pex.SqlState}, {pex.Message}");
        }
        catch (Exception ex)
        {
            ex.Log();
            var tex = PostgreSqlServerProvider.PostgreServerExceptionTranslator.Translate(ex) ?? ex;
            tex.Log();
            throw tex;
        }
    }

    public void CreateOrUpdateSchema(string schemaName)
    {
        schemaName = PostgreHelper.CheckObjectName(schemaName);

        this.CheckConnection();
        if (this.IsSchemaAvailable(schemaName))
        {
            return;
        }

        try
        {
            string sql = this.DdlGenerator.CreateSchema(schemaName);
            this.Connection.Execute(sql);
        }
        catch (PostgresException pex) when (pex.SqlState == "42P06") // duplicate_schema
        {
            throw pex;
        }
        catch (PostgresException pex) when (pex.SqlState == "42501") // insufficient_privilege
        {
            pex.Log();
            throw new Exception($"Db Permission denied: {pex.SqlState}, {pex.Message}");
        }
        catch (Exception ex)
        {
            var tex = PostgreSqlServerProvider.PostgreServerExceptionTranslator.Translate(ex) ?? ex;
            tex.Log();
            throw tex;
        }
    }

    protected void ApplyStructureInternal(IEnumerable<IDbEntityMetadata> ems, bool applyScopedData = false)
    {
        HashSet<IDbEntityMetadata> appliedMetadatas = new HashSet<IDbEntityMetadata>();
        List<IDbEntityMetadata> applyRequiredMetadatas = new List<IDbEntityMetadata>();

        IEnumerable<IDbEntityMetadata> applyRequiredMetadatas2 = new IDbEntityMetadata[0];
        do
        {
            foreach (IDbEntityMetadata em in applyRequiredMetadatas)
            {
                if (!this.CanApplyToSchema(em) || appliedMetadatas.Contains(em))
                    continue;

                this.ApplyEntityStructureInternal(em, ref applyRequiredMetadatas, applyScopedData);
                appliedMetadatas.Add(em);
            }

            applyRequiredMetadatas2 = new HashSet<IDbEntityMetadata>(applyRequiredMetadatas.Except(appliedMetadatas));
        }
        while (applyRequiredMetadatas2.Any());
    }

    public void ApplyAllStructure()
    {
        IDbSchemaScope scope = this.ParentDbProvider.ParentScope.NotNull("Parent scope is null");
        HashSet<IDbEntityMetadata> appliedMetadatas = new HashSet<IDbEntityMetadata>();
        List<IDbEntityMetadata> applyRequiredMetadatas = new List<IDbEntityMetadata>();

        var allEms = scope.ParentDbScope.Metadata.GetAll();
        foreach (var em in allEms)
        {
            if (!this.CanApplyToSchema(em))
                continue;

            em.ApplyBehaviors();
        }

        allEms = scope.ParentDbScope.Metadata.GetAll();
        foreach (var em in allEms)
        {
            if (!this.CanApplyToSchema(em))
                continue; //Sadece base schema'da olanları dışarıda tutuyoruz.

            this.ApplyEntityStructureInternal(em, ref applyRequiredMetadatas, false);
            appliedMetadatas.Add(em);
        }


        this.ApplyStructureInternal(applyRequiredMetadatas, true);

        //Apply predefined data ...
        scope.ParentDbScope.Metadata.Data.Apply(scope).Wait();

        foreach (var em in allEms)
        {
            em.ApplyToScope(this.ParentScope);
        }
    }

    protected Dictionary<string, DbVariableType> GetColumnMetadataFromTable(IDbEntityMetadata em)
    {
        Dictionary<string, DbVariableType> props = new(StringComparer.InvariantCultureIgnoreCase);

        this.CheckConnection();
        string sql = this.DdlGenerator.GetTableStructure(this.ParentDbProvider.ParentScope.SchemaName, em);
        DataTable table = this.Connection.Execute(sql);
        foreach (DataRow row in table.Rows)
        {
            string columnName = row["column_name"].ToString();
            string dataType = row["data_type"].ToString();
            if (dataType.ToUpper() == "USER-DEFINED")
            {
                dataType = row["udt_name"].ToString();
            }


            string characterMaximumLength = row["character_maximum_length"]?.ToString();
            if (characterMaximumLength.IsNullOrEmpty())
                characterMaximumLength = null;
            string numericPrecision = row["numeric_precision"]?.ToString();
            string numericScale = row["numeric_scale"]?.ToString();

            DbVariableType columnMetadataOnDb = new DbVariableType();

            switch (dataType)
            {
                case "integer":
                case "serial":
                    columnMetadataOnDb.DbType = DbFieldType.Int32;
                    columnMetadataOnDb.Lenght = 0;
                    columnMetadataOnDb.Scale = 0;
                    break;
                case "smallint":
                    columnMetadataOnDb.DbType = DbFieldType.Int16;
                    columnMetadataOnDb.Lenght = 0;
                    columnMetadataOnDb.Scale = 0;
                    break;
                case "bigint":
                case "bigserial":
                    columnMetadataOnDb.DbType = DbFieldType.Int64;
                    columnMetadataOnDb.Lenght = 0;
                    columnMetadataOnDb.Scale = 0;
                    break;
                case "character varying":
                case "varchar":
                case "text":
                case "character":
                case "char":
                    columnMetadataOnDb.DbType = DbFieldType.String;
                    columnMetadataOnDb.Lenght = characterMaximumLength != null ? int.Parse(characterMaximumLength) : 0;
                    columnMetadataOnDb.Scale = 0;
                    break;
                case "timestamp":
                case "timestamp without time zone":
                case "timestamp with time zone":
                case "date":
                case "time":
                    columnMetadataOnDb.DbType = DbFieldType.DateTime;
                    columnMetadataOnDb.Lenght = 0;
                    columnMetadataOnDb.Scale = 0;
                    break;
                case "boolean":
                    columnMetadataOnDb.DbType = DbFieldType.Boolean;
                    columnMetadataOnDb.Lenght = 0;
                    columnMetadataOnDb.Scale = 0;
                    break;
                case "numeric":
                case "decimal":
                    columnMetadataOnDb.DbType = DbFieldType.Decimal;
                    columnMetadataOnDb.Lenght = numericPrecision != null ? int.Parse(numericPrecision) : 0;
                    columnMetadataOnDb.Scale = numericScale != null ? int.Parse(numericScale) : 0;
                    break;
                case "double precision":
                    columnMetadataOnDb.DbType = DbFieldType.Double;
                    columnMetadataOnDb.Lenght = 0;
                    columnMetadataOnDb.Scale = 0;
                    break;
                case "uuid":
                    columnMetadataOnDb.DbType = DbFieldType.Guid;
                    columnMetadataOnDb.Lenght = 0;
                    columnMetadataOnDb.Scale = 0;
                    break;
                case "bytea":
                    columnMetadataOnDb.DbType = DbFieldType.Binary;
                    columnMetadataOnDb.Lenght = characterMaximumLength != null ? int.Parse(characterMaximumLength) : 0;
                    columnMetadataOnDb.Scale = 0;
                    break;
                case "json":
                case "jsonb":
                    columnMetadataOnDb.DbType = DbFieldType.Object;
                    columnMetadataOnDb.Lenght = 0;
                    columnMetadataOnDb.Scale = 0;
                    break;
                case "vector":
                    columnMetadataOnDb.DbType = DbFieldType.Vector;
                    columnMetadataOnDb.Lenght = 1024; //??
                    columnMetadataOnDb.Scale = 0;
                    break;
                default:
                    throw new Exception($"Unknown data type: {dataType}");
            }

            props.Set(columnName, columnMetadataOnDb);
        }

        return props;
    }

    protected bool IsChangeRequired(DbVariableType cmOnMetadata, DbVariableType cmOnTable)
    {
        switch (cmOnMetadata.DbType)
        {
            case DbFieldType.String:
                return cmOnTable.DbType != cmOnMetadata.DbType || (cmOnTable.Lenght < cmOnMetadata.Lenght && cmOnTable.Lenght != -1);
            case DbFieldType.Decimal:
                return cmOnTable.DbType != cmOnMetadata.DbType || (cmOnTable.Lenght < cmOnMetadata.Lenght && cmOnTable.Lenght != -1) || cmOnTable.Scale != cmOnMetadata.Scale;
            default:
                return cmOnTable.DbType != cmOnMetadata.DbType;
        }
    }

    protected void UpdateStructure(IDbEntityMetadata em)
    {
        Dictionary<string, DbVariableType> tableMetadata = this.GetColumnMetadataFromTable(em);

        string tableName = PostgreHelper.TableName(this.ParentDbProvider.ParentScope.SchemaName, em.TableName);

        foreach (IDbFieldMetadata fm in em.Fields.Values)
            try
            {
                if (!fm.IsPersisted)
                    continue;

                DbVariableType cmOnMetadata = PostgreHelper.GetDataType(fm);
                string typeName = PostgreHelper.GetDataTypeName(cmOnMetadata);
                string columnName = PostgreHelper.CheckObjectName(fm.Name);

                DbVariableType cmOnTable = tableMetadata.Get(columnName);
                if (cmOnTable != null)
                {
                    if (IsChangeRequired(cmOnMetadata, cmOnTable))
                    {
                        StringBuilder alterColumnSql = new StringBuilder();

                        alterColumnSql.Append($"ALTER TABLE {tableName} ALTER COLUMN {columnName} TYPE {typeName}");

                        this.Connection.Execute(alterColumnSql.ToString());
                    }
                }
                else
                {
                    Log.Debug($"Add column: {fm.Name}");

                    StringBuilder addColumnSql = new StringBuilder();
                    addColumnSql.Append($"ALTER TABLE {tableName} ADD COLUMN {columnName} {typeName}");

                    this.Connection.Execute(addColumnSql.ToString());
                }
            }
            catch (Exception ex)
            {
                var tex = PostgreSqlServerProvider.PostgreServerExceptionTranslator.Translate(ex) ?? ex;
                tex.Log();
            }
    }

    protected void ApplyEntityStructureInternal(IDbEntityMetadata em, ref List<IDbEntityMetadata> applyRequiredMetadatas, bool applyScopedData = false)
    {
        em.NotNull();

        if (em.IsPremature)
            throw new InvalidOperationException($"Entity metadata '{this.ParentDbProvider.ParentScope.SchemaName}/{em.Name}' is premature");

        if (!this.CanApplyToSchema(em))
            throw new InvalidOperationException($"Entity {em.Name} can't apply this schema ({this.ParentScope.SchemaName}) (marked OnlyBaseSchema??)");

        IUpdateResult bres = em.ApplyBehaviors();

        try
        {
            string schemaName = PostgreHelper.CheckObjectName(this.ParentDbProvider.ParentScope.SchemaName);
            string tableName = PostgreHelper.CheckObjectName(em.TableName);

            //string typeSql = this.DdlGenerator.CreateTableTypeDeclaration(this.ParentDbProvider.ParentScope.SchemaName, em);
            //this.Connection.Execute(typeSql);

            if (this.IsExists(schemaName, tableName))
            {
                this.UpdateStructure(em);
            }
            else
            {
                string sql = this.DdlGenerator.CreateTable(schemaName, em);
                this.Connection.Execute(sql);
            }

            if (bres.AddedItems.Any())
            {
                foreach (object obj in bres.AddedItems)
                {
                    if (obj is IDbEntityMetadata aem)
                    {
                        this.ApplyEntityStructureInternal(aem, ref applyRequiredMetadatas);
                    }
                }
            }

            //TODO: Patch, reengineering with IDataType.StructureUpdate ...
            //-----------------------------------------------------------
            if (em.Fields.Values.Any(fm => fm is VirtualRelationN2NDbFieldMetadata))
            {
                var n2nFms = em.Fields.Values.Where(fm => fm is VirtualRelationN2NDbFieldMetadata);
                foreach (VirtualRelationN2NDbFieldMetadata fm in n2nFms)
                {
                    var refEm = this.ParentScope.ParentDbScope.Metadata.Get(fm.JunctionEntityName);
                    if (this.CanApplyToSchema(refEm))
                        applyRequiredMetadatas.Add(refEm);
                }
            }

            if (em.Fields.Values.Any(fm => fm is ReferenceDbFieldMetadata))
            {
                var n2nFms = em.Fields.Values.Where(fm => fm is ReferenceDbFieldMetadata);
                foreach (ReferenceDbFieldMetadata fm in n2nFms)
                {
                    var refEm = this.ParentScope.ParentDbScope.Metadata.Get(fm.ReferencedEntity);
                    if (this.CanApplyToSchema(refEm))
                        applyRequiredMetadatas.Add(refEm);
                }
            }
            //-----------------------------------------------------------

            if (applyScopedData)
            {
                em.ApplyToScope(this.ParentDbProvider.ParentScope);
            }
        }
        catch (PostgresException pex) when (pex.SqlState == "42P07") // duplicate_table
        {
            throw pex;
        }
        catch (PostgresException pex) when (pex.SqlState == "42501") // insufficient_privilege
        {
            pex.Log();
            throw new Exception($"Db Permission denied: {pex.SqlState}, {pex.Message}");
        }
        catch (Exception ex)
        {
            var tex = PostgreSqlServerProvider.PostgreServerExceptionTranslator.Translate(ex) ?? ex;
            tex.Log();
            throw tex;
        }
    }

    public void ApplyEntityStructure(IDbEntityMetadata em, bool applyScopedData = false)
    {
        em.NotNull();

        HashSet<IDbEntityMetadata> appliedMetadatas = new HashSet<IDbEntityMetadata>();
        List<IDbEntityMetadata> applyRequiredMetadatas = new List<IDbEntityMetadata>();

        this.ApplyEntityStructureInternal(em, ref applyRequiredMetadatas, applyScopedData);
        appliedMetadatas.Add(em);

        var applyRequiredMetadatas2 = new HashSet<IDbEntityMetadata>(applyRequiredMetadatas.Except(appliedMetadatas));

        this.ApplyStructureInternal(applyRequiredMetadatas, applyScopedData);
    }

    public void DropEntity(IDbEntityMetadata em)
    {
        if (this.IsExists(this.ParentDbProvider.ParentScope.SchemaName, em.TableName))
        {
            string sql = this.DdlGenerator.DropTable(this.ParentDbProvider.ParentScope.SchemaName, em.TableName);
            this.Connection.Execute(sql);
        }
    }

    public void DestroyDatabase(string dbName)
    {
        this.CheckConnection();
        // No master database in PostgreSQL, so just connect and drop
        try
        {
            string sql1 = this.DdlGenerator.DropDatabase01(dbName);
            this.Connection.Execute(sql1);

            string sql2 = this.DdlGenerator.DropDatabase02(dbName);
            this.Connection.Execute(sql2);
        }
        catch (PostgresException pex) when (pex.SqlState == "3D000") // invalid_catalog_name
        {
            throw pex;
        }
        catch (PostgresException pex) when (pex.SqlState == "42501") // insufficient_privilege
        {
            pex.Log();
            throw new Exception($"Db Permission denied: {pex.SqlState}, {pex.Message}");
        }
        catch (Exception ex)
        {
            var tex = PostgreSqlServerProvider.PostgreServerExceptionTranslator.Translate(ex) ?? ex;
            tex.Log();
            throw tex;
        }
    }

    public void DestroySchema(string schemaName)
    {
        string sql1 = this.DdlGenerator.DropSchema(schemaName);
        this.Connection.Execute(sql1);
    }

    public void CreateSequenceIfNotExists(string name, int minValue = -1, int startValue = -1)
    {
        var seq = new PostgreSqlSequence((PostgreSqlServerDataModificationProvider)this.ParentDbProvider.GetDataModificationPovider(), name);
        seq.CreateIfNotExists(minValue, startValue);
    }

    public string CheckObjectName(string name)
    {
        name.NotNull();
        // PostgreSQL allows lowercase names, but we use a consistent format
        return PostgreHelper.CheckObjectName(name);
    }
}
