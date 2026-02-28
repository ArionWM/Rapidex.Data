using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using Rapidex.Data.PostgreServer;
using SqlKata;
using SqlKata.Compilers;
using Superpower.Model;
using static Rapidex.Data.DbEntityFactory;

namespace Rapidex.Data.PostgreServer;

internal class PostgreSqlServerDataModificationProvider : IDbDataModificationPovider, IDisposable
{
    protected PostgreSqlDdlGenerator DdlGenerator { get; set; } = new PostgreSqlDdlGenerator();

    internal PostgreSqlServerConnection Connection { get; set; }

    internal string ConnectionString { get; set; }

    public IDbSchemaScope ParentScope { get; }

    public IDbInternalTransactionScope CurrentTransaction { get; protected set; }

    public IDbProvider ParentProvider { get; protected set; }


    public PostgreSqlServerDataModificationProvider(IDbSchemaScope parentScope, IDbProvider parentProvider, string connectionString)
    {
        this.ParentScope = parentScope;
        this.ParentProvider = parentProvider;
        this.ConnectionString = connectionString;

        this.CheckConnection();
    }

    protected void CloseConnection()
    {
        if (this.Connection != null)
        {
            this.Connection.Dispose();
            this.Connection = null;
        }
    }

    internal bool CheckConnection(bool tryMaster = false)
    {
        try
        {
            if (this.Connection == null)
                this.Connection = new PostgreSqlServerConnection(this.ConnectionString);

            return true;
        }
        catch (Exception ex)
        {
            var tex = PostgreSqlServerProvider.PostgreServerExceptionTranslator.Translate(ex) ?? ex;
            tex.Log();
            throw tex;
        }
    }

    public SqlKata.Compilers.Compiler GetCompiler()
    {
        return new PostgresCompiler();
    }

    public IDbInternalTransactionScope BeginTransaction(string transactionName = null)
    {
        if (this.CurrentTransaction != null && this.CurrentTransaction.Live)
            throw new InvalidOperationException("Transaction already active");

        this.CurrentTransaction = new PostgreSqlInternalTransactionScope(this.Connection);
        return this.CurrentTransaction;
    }

    public async Task<IEntityLoadResult> Load(IDbEntityMetadata em, IEnumerable<DbEntityId> ids)
    {
        var query = this.ParentScope.GetQuery(em);

        var idArray = ids.Select(id => id.Id).ToArray();

        string fieldName = this.ParentScope.Structure.CheckObjectName(em.PrimaryKey.Name);

        query.Query.WhereIn(fieldName, idArray); //TODO: Temp table for large data

        return await this.Load(query);
    }

    public async Task<IEntityLoadResult> Load(IDbEntityMetadata em, IQueryLoader loader, SqlResult compiledSql)
    {
        this.CheckConnection();
        string sql = compiledSql.Sql;
        DbVariable[] variables = DbVariable.Get(compiledSql.NamedBindings);

#if DEBUG
        Common.DefaultLogger?.LogDebug($"{sql} \r\n {variables.Select(v => $"{v.ParameterName}: {v.Value} ({v.Value?.GetType()})")}");
#endif

        DataTable table = await this.Connection.Execute(sql, variables);

        IEntity[] entities = this.ParentScope.Mapper.MapToNew(em, table, ent => { ent._loadSource = LoadSource.Database; });
        return new EntityLoadResult(entities);
    }

    public async Task<IEntityLoadResult> Load(IQueryLoader loader)
    {
        var compiler = this.GetCompiler();
        SqlResult result = compiler.Compile(loader.Query);

        return await this.Load(loader.EntityMetadata, loader, result);
    }

    public async Task<ILoadResult<DataRow>> LoadRaw(IQueryLoader loader, SqlResult compiledSql)
    {
        this.CheckConnection();

        string sql = compiledSql.Sql;
        DbVariable[] variables = DbVariable.Get(compiledSql.NamedBindings);

#if DEBUG
        Common.DefaultLogger?.LogDebug($"{sql} \r\n {variables.Select(v => $"{v.ParameterName}: {v.Value} ({v.Value?.GetType()})")}");
#endif

        DataTable table = await this.Connection.Execute(sql, variables);
        return new LoadResult<DataRow>(table.Rows.AsEnumerable());
    }

    public async Task<ILoadResult<DataRow>> LoadRaw(IQueryLoader loader)
    {
        this.CheckConnection();

        var compiler = this.GetCompiler();
        SqlResult result = compiler.Compile(loader.Query);
        return await this.LoadRaw(loader, result);
    }

    protected DbVariable GetDbVariable(IDbFieldMetadata fm, IEntity entity, string parameterPostfix)
    {
        object lowerValue = fm.ValueGetterLower(entity, fm.Name);
        DbVariable dbVariable = PostgreHelper.GetData(fm, lowerValue);
        if (!string.IsNullOrEmpty(parameterPostfix))
            dbVariable.ParameterName += parameterPostfix;

        return dbVariable;
    }

    protected DbVariable[] GetDbVariables(IDbEntityMetadata em, IEntity entity, string parameterPostfix, params string[] excludes)
    {
        List<DbVariable> dbVariables = new List<DbVariable>();
        foreach (IDbFieldMetadata fm in em.Fields.Values)
        {
            if (!fm.IsPersisted || excludes.Contains(fm.Name))
                continue;

            DbVariable dbVariable = this.GetDbVariable(fm, entity, parameterPostfix);

            dbVariables.Add(dbVariable);
        }

#if DEBUG
        Common.DefaultLogger?.LogDebug(dbVariables.ToLogStr());
#endif

        return dbVariables.ToArray();
    }

    protected DataTable GetDbVariableTable(string schemaName, IDbEntityMetadata em, string parameterPostfix)
    {
        DataTable table = new DataTable();
        schemaName = PostgreHelper.CheckObjectName(schemaName);
        string tableName = PostgreHelper.CheckObjectName(em.TableName);
        table.TableName = $"{tableName}";

        var sortedFields = em.Fields.Values.OrderBy(f => f.Name);

        DbVariable dbVariableId = PostgreHelper.GetData(em.PrimaryKey, null);
        if (!string.IsNullOrEmpty(parameterPostfix))
            dbVariableId.ParameterName += parameterPostfix;

        var primaryKeyColumn = table.Columns.Add(PostgreHelper.CheckObjectName(dbVariableId.FieldName), em.PrimaryKey.BaseType);
        table.PrimaryKey = primaryKeyColumn.CreateArray();

        foreach (IDbFieldMetadata fm in sortedFields)
        {
            if (!fm.IsPersisted)
                continue;

            if (fm == em.PrimaryKey)
                continue;

            DbVariable dbVariable = PostgreHelper.GetData(fm, null);
            if (!string.IsNullOrEmpty(parameterPostfix))
                dbVariable.ParameterName += parameterPostfix;

            string columnName = PostgreHelper.CheckObjectName(dbVariable.FieldName);
            table.Columns.Add(columnName, fm.BaseType);
        }

        return table;
    }

    protected DbVariable[] GetDbVariables(IDbEntityMetadata em, IPartialEntity entity, string parameterPostfix, params string[] excludes)
    {
        PartialEntity partialEntity = (PartialEntity)entity;
        ObjDictionary values = partialEntity.GetAllValues();

        List<DbVariable> dbVariables = new List<DbVariable>();

        foreach (var kv in values)
        {
            IDbFieldMetadata fm = em.Fields[kv.Key];
            if (!fm.IsPersisted || excludes.Contains(fm.Name))
                continue;

            DbVariable dbVariable = this.GetDbVariable(fm, entity, parameterPostfix);
            dbVariables.Add(dbVariable);
        }

#if DEBUG
        Common.DefaultLogger?.LogDebug(dbVariables.ToLogStr());
#endif

        return dbVariables.ToArray();
    }

    [Obsolete("Disable for bulk update issue", true)]
    protected async Task<IEntityUpdateResult> InsertV2(IDbEntityMetadata em, IEnumerable<IEntity> entities)
    {
        this.CheckConnection();

        EntityUpdateResult result = new EntityUpdateResult();

        TemplateInfo info = Database.EntityFactory.GetTemplate(em, this.ParentScope);
        string schemaName = PostgreHelper.CheckObjectName(this.ParentScope.SchemaName);

        int requiredIdCount = entities.Count(ent => ((long)ent.GetId()).IsPrematureOrEmptyId());
        long[] ids = new long[0];
        if (requiredIdCount > 0)
        {
            if (requiredIdCount == 1)
                ids = new long[] { await this.ParentScope.Data.Sequence(info.PersistentSequence).GetNext() };
            else
                ids = await this.ParentScope.Data.Sequence(info.PersistentSequence).GetNextN(requiredIdCount);
        }

        DataTable variableTable = this.GetDbVariableTable(schemaName, em, null);

        int idCount = 0;
        foreach (IEntity entity in entities)
        {
            if (entity is IPartialEntity)
                throw new InvalidOperationException($"Partial entities cannot be inserted ('{entity.GetType().Name}')");

            EntityChangeResultItem ures = new EntityChangeResultItem();
            ures.Name = em.Name;
            long oldId = entity.GetId() is long oldIdLong && oldIdLong < 0 ? oldIdLong : entity._VirtualId.As<long>();

            ures.OldId = oldId;
            ures.Id = (long)entity.GetId();

            try
            {
                if (ures.Id.IsPersistedRecordId())
                    continue;

                long id = ids[idCount];
                idCount++;
                ures.Id = id;
                entity.SetId(id);
            }
            finally
            {
                result.Added(ures);
            }
        }

        foreach (IEntity entity in entities)
        {
            DataRow row = variableTable.NewRow();
            foreach (IDbFieldMetadata fm in em.Fields.Values)
            {
                if (!fm.IsPersisted)
                    continue;

                object lowerValue = fm.ValueGetterLower(entity, fm.Name);
                string columnName = PostgreHelper.CheckObjectName(fm.Name);
                row[columnName] = PostgreHelper.CheckValue(lowerValue);
            }
            variableTable.Rows.Add(row);
        }

        await this.Connection.BulkUpdate(schemaName, variableTable);

        foreach (IEntity entity in entities)
        {
            entity._IsNew = false;
        }

        return result;
    }

    protected async Task SimpleInsertBatch(IDbEntityMetadata em, DbVariable[] majorVariables, IEnumerable<IEntity> entities)
    {
        List<DbVariable[]> fields = new List<DbVariable[]>();
        List<DbVariable> flatFields = new List<DbVariable>();

        for (int i = 0; i < entities.Count(); i++)
        {
            IEntity entity = entities.ElementAt(i);
            DbVariable[] variables = this.GetDbVariables(em, entity, i.ToString("0000"));
            fields.Add(variables);
            flatFields.AddRange(variables);
        }

        //max parameter count: 65535, split 

        string sql = this.DdlGenerator.Insert(this.ParentScope.SchemaName, em.TableName, majorVariables, fields);

        await this.Connection.Execute(sql, flatFields.ToArray());
    }

    protected async Task<IEntityUpdateResult> SimpleInsert(IDbEntityMetadata em, IEnumerable<IEntity> entities)
    {
        this.CheckConnection();

        EntityUpdateResult result = new EntityUpdateResult();

        TemplateInfo info = Database.EntityFactory.GetTemplate(em, this.ParentScope);

        int requiredIdCount = entities.Count(ent => ((long)ent.GetId()).IsPrematureOrEmptyId());
        long[] ids = new long[0];
        if (requiredIdCount > 0)
        {
            if (requiredIdCount == 1)
                ids = new long[] { await this.ParentScope.Data.Sequence(info.PersistentSequence).GetNext() };
            else
                ids = await this.ParentScope.Data.Sequence(info.PersistentSequence).GetNextN(requiredIdCount);
        }

        int idCount = 0;
        foreach (IEntity entity in entities)
        {
            if (entity is IPartialEntity)
                throw new InvalidOperationException($"Partial entities cannot be inserted ('{entity.GetType().Name}')");

            EntityChangeResultItem ures = new EntityChangeResultItem();
            ures.Name = em.Name;
            long oldId = entity.GetId() is long oldIdLong && oldIdLong < 0 ? oldIdLong : entity._VirtualId.As<long>();

            ures.OldId = oldId;
            ures.Id = (long)entity.GetId();

            try
            {
                if (ures.Id.IsPersistedRecordId())
                    continue;

                long id = ids[idCount];
                idCount++;
                ures.Id = id;
                entity.SetId(id);
            }
            finally
            {
                result.Added(ures);
            }
        }

        int count = entities.Count();

        if (count > 1)
        {
            DbVariable[] majorVariables = this.GetDbVariables(em, entities.First(), null);

            //Max parameter count: 10922 (~ 65535 / 6 )
            int rowsPerBatch = 10922 / majorVariables.Length;
            var batchs = entities.Split(rowsPerBatch);

            foreach (var batch in batchs)
            {
                await this.SimpleInsertBatch(em, majorVariables, batch);
            }
        }
        else
        {
            IEntity entity = entities.First();
            DbVariable[] variables = this.GetDbVariables(em, entity, null);

            string sql = this.DdlGenerator.Insert(this.ParentScope.SchemaName, em.TableName, variables);
            await this.Connection.Execute(sql, variables);
        }

        foreach (IEntity entity in entities)
        {
            entity._IsNew = false;
        }

        return result;
    }

    protected async Task Update(IDbEntityMetadata em, IEntity entity)
    {
        this.CheckConnection();

        DbVariable[] variables;

        if (entity is IPartialEntity pent)
            variables = this.GetDbVariables(em, pent, null);
        else
            variables = this.GetDbVariables(em, entity, null);

        DbVariable idVariable = this.GetDbVariable(em.PrimaryKey, entity, null);

        string sql = this.DdlGenerator.Update(this.ParentScope.SchemaName, em, idVariable, variables, em.PrimaryKey.Name);
        DataTable table = await this.Connection.Execute(sql, variables);
        int updatedRecordCount = 0;
        updatedRecordCount = table.Rows[0].To<int>(0);

        updatedRecordCount.MustBe(count => count == 1,
            string.Format("Entity '{0}' can't update. Possible not found with id: {1}", entity, entity.GetId()));
    }

    protected async Task<IEntityUpdateResult> Update(IDbEntityMetadata em, IEnumerable<IEntity> entities)
    {
        this.CheckConnection();

        EntityUpdateResult result = new EntityUpdateResult();

        foreach (IEntity entity in entities)
        {
            if (entity is IPartialEntity pentity && !pentity.IsAnyValueContained())
                continue;

            await this.Update(em, entity);
            entity.DbVersion++;
            result.Modified(entity);
        }

        return result;
    }

    public async Task<IEntityUpdateResult> InsertOrUpdate(IDbEntityMetadata em, IEnumerable<IEntity> entities)
    {
        this.CheckConnection();

        EntityUpdateResult result = new EntityUpdateResult();

        var division = entities.GroupBy(ent => ent._IsNew);

        var newEntities = division.FirstOrDefault(d => d.Key == true);
        var updatedEntities = division.FirstOrDefault(d => d.Key == false);

        if (newEntities != null && newEntities.Any())
        {
            result.MergeWith(await this.SimpleInsert(em, newEntities));
        }

        if (updatedEntities != null && updatedEntities.Any())
        {
            result.MergeWith(await this.Update(em, updatedEntities));
        }

        return result;
    }

    public IIntSequence Sequence(string name)
    {
        this.CheckConnection();

        return new PostgreSqlSequence(this, name);
    }

    protected async Task<IEntityUpdateResult> DeleteInternal(IDbEntityMetadata em, IEnumerable<long> ids)
    {
        this.CheckConnection();
        EntityUpdateResult result = new EntityUpdateResult();

        string sql = this.DdlGenerator.Delete(this.ParentScope.SchemaName, em, ids);
        await this.Connection.Execute(sql);

        foreach (long id in ids)
        {
            result.Deleted(em.Name, id);
        }
        return result;
    }

    public async Task<IEntityUpdateResult> Delete(IDbEntityMetadata em, IEnumerable<long> ids)
    {
        EntityUpdateResult result = new EntityUpdateResult();
        var parts = ids.Split(10);
        foreach (var part in parts)
            result.MergeWith(
              await this.DeleteInternal(em, part));

        return result;
    }

    public async Task<IEntityUpdateResult> BulkUpdate(IDbEntityMetadata em, IQueryUpdater query)
    {
        this.CheckConnection();

        SqlKata.Query _updateQuery = query.Query.Clone();
        _updateQuery.ClearComponent("select");
        _updateQuery.AsUpdate(query.UpdateData);

        var compiler = this.GetCompiler();
        SqlResult result = compiler.Compile(_updateQuery);
        string sql = result.ToString();

        // PostgreSQL does not support OUTPUT INSERTED.Id; you may need to use RETURNING
        string topPart, wherePart;

        int whereIndex = sql.IndexOf("where", StringComparison.InvariantCultureIgnoreCase);
        if (whereIndex > -1)
        {
            topPart = sql.Substring(0, whereIndex);
            wherePart = sql.Substring(whereIndex);
        }
        else
        {
            topPart = sql;
            wherePart = null;
        }

        //Add RETURNING
        string returningPart = " RETURNING id ";
        sql = topPart + wherePart + returningPart;
        DataTable table = await this.Connection.Execute(sql);

        EntityUpdateResult uResult = new EntityUpdateResult();
        foreach (DataRow row in table.Rows)
        {
            long id = row.To<long>("Id");
            uResult.Modified(id);
        }

        return uResult;
    }

    public void Dispose()
    {
        this.CloseConnection();

    }
}
