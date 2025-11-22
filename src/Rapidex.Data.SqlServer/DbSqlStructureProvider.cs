using Microsoft.Data.SqlClient;
using Rapidex.Data;
using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Rapidex.Data.RelationN2N;
using static System.Formats.Asn1.AsnWriter;

namespace Rapidex.Data.SqlServer;

public class DbSqlStructureProvider : IDbStructureProvider
{
    private readonly ILogger<DbSqlStructureProvider> logger;
    protected bool isInitialized = false;
    protected SqlConnectionStringBuilder Connectionbuilder { get; set; }
    protected DbSqlDdlGenerator DdlGenerator { get; set; } = new DbSqlDdlGenerator();
    internal DbSqlServerConnection Connection { get; set; }

    public string ConnectionString { get; protected set; }
    public IDbProvider ParentDbProvider { get; protected set; }

    public IDbSchemaScope ParentScope => this.ParentDbProvider?.ParentScope;

    public DbSqlStructureProvider(ILogger<DbSqlStructureProvider> logger)
    {
        this.logger = logger;
    }

    public void Initialize(IDbProvider parent, string connectionString)
    {
        if (this.isInitialized)
            return;

        this.ParentDbProvider = parent;
        this.ConnectionString = connectionString;
        this.isInitialized = true;
    }

    protected void Checkinitialized()
    {
        if (!this.isInitialized)
            throw new InvalidOperationException("Provider is not initialized");
    }

    protected void SwitchConnectionToMaster()
    {
        try
        {
            this.CloseConnection();

            SqlConnectionStringBuilder cbuilder = new SqlConnectionStringBuilder(this.ConnectionString);
            cbuilder.InitialCatalog = "master";
            this.Connection = new DbSqlServerConnection(cbuilder.ConnectionString);
        }
        catch (Exception ex)
        {
            var tex = DbSqlServerProvider.SqlServerExceptionTranslator.Translate(ex) ?? ex;
            tex.Log();
            throw tex;
        }
    }

    protected void CloseConnection()
    {
        if (this.Connection != null)
        {
            this.Connection.Dispose();
            this.Connection = null;
        }
    }

    //dbName parametrelerine gerek yok? Zaten parent'dan alınabilir.
    protected bool CheckConnection(bool tryMaster = false)
    {
        try
        {
            if (this.Connectionbuilder == null)
                this.Connectionbuilder = new SqlConnectionStringBuilder(this.ConnectionString);

            if (this.Connection == null)
                this.Connection = new DbSqlServerConnection(((IDbStructureProvider)this).ConnectionString);

            return true;
        }
        catch (SqlException ssx) when (ssx.Number == 4060 || ssx.Number == 233)
        {
            if (!tryMaster)
                throw;

            this.SwitchConnectionToMaster();
            return false;
        }
        catch (Exception ex)
        {
            var tex = DbSqlServerProvider.SqlServerExceptionTranslator.Translate(ex) ?? ex;
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
        this.Checkinitialized();
        this.CheckConnection(false);

        if (!this.IsDatabaseAvailable(dbName))
        {
            this.CreateDatabase(dbName);
        }

        string sql = this.DdlGenerator.SwitchDatabase(dbName);
        DataTable dataTable = this.Connection.Execute(sql);
    }


    public bool IsDatabaseAvailable(string dbName)
    {
        this.Checkinitialized();
        bool directTargetDatabase = this.CheckConnection(true);
        string sql = this.DdlGenerator.IsDatabaseAvailable(dbName);
        DataTable dataTable = this.Connection.Execute(sql);
        bool isAvail = dataTable.Rows.Count > 0 && dataTable.Rows[0]["DatabaseId"] != DBNull.Value;
        return isAvail;
    }

    public bool IsExists(string schemaName, string entityName)
    {
        this.Checkinitialized();
        this.CheckConnection();
        string sql = this.DdlGenerator.IsTableAvailable(schemaName, entityName);
        //TODO: Daha hızlı bir yol? tüm exists'ler için ...
        DataTable dataTable = this.Connection.Execute(sql);
        bool isAvail = dataTable.Rows.Count > 0 && dataTable.Rows[0]["TableId"] != DBNull.Value;
        return isAvail;
    }

    public bool IsExists(string schemaName, string entityName, IDbFieldMetadata cm)
    {
        this.Checkinitialized();
        this.CheckConnection();
        string sql = this.DdlGenerator.IsColumnAvailable(schemaName, entityName, cm.Name);
        DataTable dataTable = this.Connection.Execute(sql);
        bool isAvail = dataTable.Rows.Count > 0 && dataTable.Rows[0]["TableId"] != DBNull.Value;
        return isAvail;
    }

    public bool IsSchemaAvailable(string schemaName)
    {
        this.Checkinitialized();
        this.CheckConnection();
        string sql = this.DdlGenerator.IsSchemaAvailable(schemaName);
        DataTable dataTable = this.Connection.Execute(sql);
        bool isAvail = dataTable.Rows.Count > 0 && dataTable.Rows[0]["SchemaId"] != DBNull.Value;
        return isAvail;
    }

    public void CreateDatabase(string dbName)
    {
        dbName.NotEmpty();

        this.Checkinitialized();
        this.CheckConnection();

        try
        {
            this.ParentDbProvider.CanCreateDatabase();

            string sql1 = this.DdlGenerator.CreateDatabase01(dbName, this.Connectionbuilder.UserID);
            this.Connection.Execute(sql1);

            string sql2 = this.DdlGenerator.CreateDatabase02(dbName, this.Connectionbuilder.UserID);
            this.Connection.Execute(sql2);

            this.SwitchDatabase(dbName);
        }
        catch (SqlException sex) when (sex.Number == 2714)
        {
            throw sex;
            //Common.DefaultLogger?.LogWarn($"Table {entityMetadata.TableName} already exists");
        }
        catch (SqlException sex) when (DbSqlServerHelper.SQL_Errors_Permission.Contains(sex.Number))
        {
            sex.Log();
            throw new Exception($"Db Permission denied: {sex.Number}, {sex.Message}");
        }
        catch (Exception ex)
        {
            ex.Log();
            var tex = DbSqlServerProvider.SqlServerExceptionTranslator.Translate(ex) ?? ex;
            tex.Log();
            throw tex;
        }
    }

    public void CreateOrUpdateSchema(string schemaName)
    {
        this.Checkinitialized();
        this.CheckConnection();

        if (this.IsSchemaAvailable(schemaName))
        {
            return;
        }

        try
        {
            this.ParentDbProvider.CanCreateSchema();

            string sql = this.DdlGenerator.CreateSchema(schemaName);
            this.Connection.Execute(sql);
        }
        catch (SqlException sex) when (sex.Number == 2714)
        {
            throw sex;
            //Common.DefaultLogger?.LogWarn($"Table {entityMetadata.Name} already exists");
        }
        catch (SqlException sex) when (DbSqlServerHelper.SQL_Errors_Permission.Contains(sex.Number))
        {
            sex.Log();
            throw new Exception($"Db Permission denied: {sex.Number}, {sex.Message}");
        }
        catch (Exception ex)
        {
            var tex = DbSqlServerProvider.SqlServerExceptionTranslator.Translate(ex) ?? ex;
            tex.Log();
            throw tex;
        }
    }

    protected void ApplyStructureInternal(IEnumerable<IDbEntityMetadata> ems, bool applyScopedData = false)
    {
        HashSet<IDbEntityMetadata> appliedMetadatas = new HashSet<IDbEntityMetadata>();
        List<IDbEntityMetadata> applyRequiredMetadatas = new List<IDbEntityMetadata>(ems);

        IEnumerable<IDbEntityMetadata> applyRequiredMetadatas2 = new IDbEntityMetadata[0];
        do
        {
            foreach (IDbEntityMetadata em in applyRequiredMetadatas.ToArray())
            {
                if (!this.CanApplyToSchema(em) || appliedMetadatas.Contains(em))
                    continue;

                this.ApplyEntityStructureInternal(em, ref applyRequiredMetadatas, applyScopedData);
                appliedMetadatas.Add(em);
            }

            applyRequiredMetadatas2 = new HashSet<IDbEntityMetadata>(applyRequiredMetadatas.Except(appliedMetadatas));
            applyRequiredMetadatas = applyRequiredMetadatas2.ToList();
        }
        while (applyRequiredMetadatas2.Any());
    }

    public void ApplyAllStructure()
    {
        try
        {
            this.logger.LogInformation("Applying database structure...");

            this.Checkinitialized();
            IDbSchemaScope scope = this.ParentDbProvider.ParentScope.NotNull("Parent scope is null");

            HashSet<IDbEntityMetadata> appliedMetadatas = new HashSet<IDbEntityMetadata>();
            List<IDbEntityMetadata> applyRequiredMetadatas = new List<IDbEntityMetadata>();

            this.ParentDbProvider.CanCreateTable(this.ParentDbProvider.ParentScope.SchemaName);

            this.logger.LogInformation("Applying behaviors to entities...");
            var allEms = scope.ParentDbScope.Metadata.GetAll();
            foreach (var em in allEms)
            {
                if (!this.CanApplyToSchema(em))
                    continue;

                this.logger.LogDebug($"Applying behaviors to entity: {em.Name}");
                em.ApplyBehaviors();
            }

            this.logger.LogInformation("Applying entity structures...");
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
            scope.ParentDbScope.Metadata.Data.Apply(scope);

            foreach (var em in allEms)
            {
                em.ApplyToScope(this.ParentScope);
            }

        }
        catch (Exception ex)
        {
            ex.Log();
            var tex = DbSqlServerProvider.SqlServerExceptionTranslator.Translate(ex) ?? ex;
            throw tex;
        }
    }


    protected Dictionary<string, DbVariableType> GetColumnMetadataFromTable(IDbEntityMetadata em)
    {
        Dictionary<string, DbVariableType> props = new Dictionary<string, DbVariableType>(StringComparer.InvariantCultureIgnoreCase);

        this.CheckConnection();
        string sql = this.DdlGenerator.GetTableStructure(this.ParentDbProvider.ParentScope.SchemaName, em);
        DataTable table = this.Connection.Execute(sql);
        foreach (DataRow row in table.Rows)
        {
            string columnName = row["COLUMN_NAME"].ToString();
            string dataType = row["DATA_TYPE"].ToString();
            string characterMaximumLength = row["CHARACTER_MAXIMUM_LENGTH"].ToString();
            string numericPrecision = row["NUMERIC_PRECISION"].ToString();
            string numericScale = row["NUMERIC_SCALE"].ToString();

            DbVariableType columnMetadataOnDb = new DbVariableType();

            //use switch case to set the columnType, lenght, Scale variables
            switch (dataType)
            {
                case "int":
                case "tinyint":
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
                    columnMetadataOnDb.DbType = DbFieldType.Int64;
                    break;
                case "nvarchar":
                case "varchar":
                case "text":
                case "nchar":
                    columnMetadataOnDb.DbType = DbFieldType.String;
                    columnMetadataOnDb.Lenght = int.Parse(characterMaximumLength);
                    columnMetadataOnDb.Scale = 0;
                    break;
                case "datetime":
                case "smalldatetime":
                case "date":
                case "time":
                case "datetime2":
                    columnMetadataOnDb.DbType = DbFieldType.DateTime;
                    columnMetadataOnDb.Lenght = 0;
                    columnMetadataOnDb.Scale = 0;
                    break;
                case "datetimeoffset":
                    columnMetadataOnDb.DbType = DbFieldType.DateTimeOffset;
                    columnMetadataOnDb.Lenght = 0;
                    columnMetadataOnDb.Scale = 0;
                    break;
                case "bit":
                    columnMetadataOnDb.DbType = DbFieldType.Boolean;
                    columnMetadataOnDb.Lenght = 0;
                    columnMetadataOnDb.Scale = 0;
                    break;
                case "decimal":
                    columnMetadataOnDb.DbType = DbFieldType.Decimal;
                    columnMetadataOnDb.Lenght = int.Parse(numericPrecision);
                    columnMetadataOnDb.Scale = int.Parse(numericScale);
                    break;
                case "float":
                    columnMetadataOnDb.DbType = DbFieldType.Double;
                    columnMetadataOnDb.Lenght = 0;
                    columnMetadataOnDb.Scale = 0;
                    break;
                case "uniqueidentifier":
                    columnMetadataOnDb.DbType = DbFieldType.Guid;
                    columnMetadataOnDb.Lenght = 0;
                    columnMetadataOnDb.Scale = 0;
                    break;
                case "image":
                case "varbinary":
                case "binary":
                    columnMetadataOnDb.DbType = DbFieldType.Binary;
                    columnMetadataOnDb.Lenght = int.Parse(characterMaximumLength);
                    columnMetadataOnDb.Scale = 0;
                    break;
                case "xml":
                    columnMetadataOnDb.DbType = DbFieldType.Xml;
                    columnMetadataOnDb.Lenght = int.Parse(characterMaximumLength);
                    columnMetadataOnDb.Scale = 0;
                    break;
                //Vector01
                default:
                    throw new Exception("Unknown data type");
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
                return cmOnTable.DbType != cmOnMetadata.DbType || (cmOnTable.Lenght < cmOnMetadata.Lenght && cmOnTable.Lenght != -1) || (cmOnMetadata.Lenght == -1 && cmOnTable.Lenght > -1 && cmOnTable.Lenght < 4000);
            case DbFieldType.Decimal:
                return cmOnTable.DbType != cmOnMetadata.DbType || (cmOnTable.Lenght < cmOnMetadata.Lenght && cmOnTable.Lenght != -1) || cmOnTable.Scale != cmOnMetadata.Scale;
            default:
                return cmOnTable.DbType != cmOnMetadata.DbType;
        }
    }

    protected void UpdateStructure(IDbEntityMetadata em)
    {
        Dictionary<string, DbVariableType> tableMetadata = this.GetColumnMetadataFromTable(em);

        string tableName = DbSqlServerHelper.DbName(this.ParentDbProvider.ParentScope.SchemaName, em.TableName);

        foreach (IDbFieldMetadata fm in em.Fields.Values)
            try
            {
                if (!fm.IsPersisted)
                    continue;

                DbVariableType cmOnMetadata = DbSqlServerHelper.GetDataType(fm);
                string typeName = DbSqlServerHelper.GetDataTypeName(cmOnMetadata);

                DbVariableType cmOnTable = tableMetadata.Get(fm.Name);
                if (cmOnTable != null)
                {
                    if (IsChangeRequired(cmOnMetadata, cmOnTable))
                    {
                        //DropIndexesAndConstraintsIsRequired(tableName, columnName);

                        StringBuilder alterColumnSql = new StringBuilder();
                        alterColumnSql.Append($"alter table {tableName} alter column ");
                        alterColumnSql.Append($"[{fm.Name}] {typeName}");

                        this.Connection.Execute(alterColumnSql.ToString());
                    }
                }
                else
                {
                    Common.DefaultLogger?.LogDebug($"Add column: {fm.Name}");

                    StringBuilder addColumnSql = new StringBuilder();
                    addColumnSql.Append($"alter table {tableName} add ");
                    addColumnSql.Append($"[{fm.Name}] {typeName}");

                    this.Connection.Execute(addColumnSql.ToString());

                    //if (fm.IsUniqueOnAllDb /*|| pm.IsUnique*/ /*WARN: Sadece tek kiracılı modelde!?*/)
                    //     this.Connection.Execute($"alter table {tableName} add  CONSTRAINT {uniqueConstraintName} UNIQUE({columnName}) ");

                    //if (pm.IsUnique)
                    //     this.Connection.Execute($"alter table {tableName} add  CONSTRAINT AK_{tableName}_{pm.ColumnName} UNIQUE({pm.ColumnName}) ");
                }
            }
            catch (Exception ex)
            {
                var tex = DbSqlServerProvider.SqlServerExceptionTranslator.Translate(ex) ?? ex;
                tex.Log();
                //exceptions.Add(ex);
            }

        //return exceptions.ToArray();
    }

    protected void ApplyEntityStructureInternal(IDbEntityMetadata em, ref List<IDbEntityMetadata> applyRequiredMetadatas, bool applyScopedData = false)
    {
        em.NotNull();

        this.logger.LogDebug($"Applying structure for entity: {em.Name}"); 

        if (em.IsPremature)
            throw new InvalidOperationException($"Entity metadata '{this.ParentDbProvider.ParentScope.SchemaName}/{em.Name}' is premature");

        if (!this.CanApplyToSchema(em))
            throw new InvalidOperationException($"Entity {em.Name} can't apply this schema ({this.ParentScope.SchemaName}) (marked OnlyBaseSchema??)");

        IUpdateResult bres = em.ApplyBehaviors();

        try
        {
            //Update type ...
            string typeSql = this.DdlGenerator.CreateTableTypeDeclaration(this.ParentDbProvider.ParentScope.SchemaName, em);
            this.Connection.Execute(typeSql);

            if (this.IsExists(this.ParentDbProvider.ParentScope.SchemaName, em.TableName))
            {
                this.UpdateStructure(em);
            }
            else
            {
                string sql = this.DdlGenerator.CreateTable(this.ParentDbProvider.ParentScope.SchemaName, em);
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
        catch (SqlException sex) when (sex.Number == 2714)
        {
            Common.DefaultLogger?.LogWarning($"Table {em.TableName} already exists");
            throw;

        }
        catch (SqlException sex) when (DbSqlServerHelper.SQL_Errors_Permission.Contains(sex.Number))
        {
            sex.Log();
            throw new Exception($"Db Permission denied: {sex.Number}, {sex.Message}");
        }
        catch (Exception ex)
        {
            var tex = DbSqlServerProvider.SqlServerExceptionTranslator.Translate(ex) ?? ex;
            tex.Log();
            throw tex;
        }
    }

    public void ApplyEntityStructure(IDbEntityMetadata em, bool applyScopedData = false)
    {
        em.NotNull();
        this.Checkinitialized();

        HashSet<IDbEntityMetadata> appliedMetadatas = new HashSet<IDbEntityMetadata>();
        List<IDbEntityMetadata> applyRequiredMetadatas = new List<IDbEntityMetadata>();

        this.ApplyEntityStructureInternal(em, ref applyRequiredMetadatas, applyScopedData);
        appliedMetadatas.Add(em);

        var applyRequiredMetadatas2 = new HashSet<IDbEntityMetadata>(applyRequiredMetadatas.Except(appliedMetadatas));

        this.ApplyStructureInternal(applyRequiredMetadatas, applyScopedData);
    }


    public void DropEntity(IDbEntityMetadata em)
    {
        this.Checkinitialized();
        if (this.IsExists(this.ParentDbProvider.ParentScope.SchemaName, em.TableName))
        {
            string sql = this.DdlGenerator.DropTable(this.ParentDbProvider.ParentScope.SchemaName, em.TableName);
            this.Connection.Execute(sql);
        }
    }


    public void DestroyDatabase(string dbName)
    {
        this.Checkinitialized();
        this.CheckConnection();
        this.SwitchDatabase("master");

        try
        {
            string sql1 = this.DdlGenerator.DropDatabase01(dbName);
            this.Connection.Execute(sql1);

            string sql2 = this.DdlGenerator.DropDatabase02(dbName);
            this.Connection.Execute(sql2);
        }
        catch (SqlException sex) when (sex.Number == 2714)
        {
            throw;
            //Common.DefaultLogger?.LogWarn($"Table {entityMetadata.TableName} already exists");
        }
        catch (SqlException sex) when (DbSqlServerHelper.SQL_Errors_Permission.Contains(sex.Number))
        {
            sex.Log();
            throw new DbInsufficientPermissionsException("Drop database", null, sex.Message);
        }
        catch (Exception ex)
        {
            var tex = DbSqlServerProvider.SqlServerExceptionTranslator.Translate(ex) ?? ex;
            tex.Log();
            throw tex;
        }
    }

    public void DestroySchema(string schemaName)
    {
        this.Checkinitialized();
        string sql1 = this.DdlGenerator.DropSchema(schemaName);
        this.Connection.Execute(sql1);
    }

    public void CreateSequenceIfNotExists(string name, int minValue = -1, int startValue = -1)
    {
        this.Checkinitialized();
        using var provider = (DbSqlServerDataModificationProvider)this.ParentDbProvider.GetDataModificationProvider();
        var seq = new DbSqlSequence(provider, name);
        seq.CreateIfNotExists(minValue, startValue);
    }

    public string CheckObjectName(string name)
    {
        return name;
    }

    public void Dispose()
    {
        this.CloseConnection();
    }

    public (MasterDbConnectionStatus status, string description) CheckMasterConnection()
    {
        try
        {
            this.Checkinitialized();
            bool directHit = this.CheckConnection(true);
            if (!directHit)
            {
                this.CreateDatabase(this.Connectionbuilder.InitialCatalog);
            }

            this.SwitchDatabase(this.Connectionbuilder.InitialCatalog);

            var status = directHit ? MasterDbConnectionStatus.Valid : MasterDbConnectionStatus.Created;
            return (status, status == MasterDbConnectionStatus.Valid ? "Valid" : "Created");
        }
        catch (Exception ex)
        {
            ex.Log();

            var tex = DbSqlServerProvider.SqlServerExceptionTranslator.Translate(ex) ?? ex;

            return (MasterDbConnectionStatus.CantAccess, tex.Message);
        }
    }
}
