using Npgsql;
using SqlKata.Compilers;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Rapidex.Data.PostgreServer;
using static Rapidex.Data.DbEntityFactory;

namespace Rapidex.Data.PostgreServer;

internal class PostgreSqlServerDataModificationProvider : IDbDataModificationPovider
{
    protected PostgreSqlDdlGenerator DdlGenerator { get; set; } = new PostgreSqlDdlGenerator();

    internal PostgreSqlServerConnection Connection { get; set; }

    internal string ConnectionString { get; set; }

    public IDbSchemaScope ParentScope { get; }

    public IDbInternalTransactionScope CurrentTransaction { get; }
    public IDbProvider ParentProvider { get; protected set; }


    public PostgreSqlServerDataModificationProvider(IDbSchemaScope parentScope, IDbProvider parentProvider, string connectionString)
    {
        this.ParentScope = parentScope;
        this.ParentProvider = parentProvider;
        this.ConnectionString = connectionString;

        this.CheckConnection();
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

    public IDbInternalTransactionScope BeginTransaction(string transactionName = null)
    {
        throw new NotImplementedException();
    }

    public IEntityLoadResult Load(IDbEntityMetadata em, IEnumerable<DbEntityId> ids)
    {
        var query = this.ParentScope.GetQuery(em);

        var idArray = ids.Select(id => id.Id).ToArray();

        string fieldName = this.ParentScope.Structure.CheckObjectName(em.PrimaryKey.Name);

        query.Query.WhereIn(fieldName, idArray);

        return this.Load(query);
    }

    public IEntityLoadResult Load(IQueryLoader loader)
    {
        this.CheckConnection();

        var compiler = new PostgresCompiler();
        SqlResult result = compiler.Compile(loader.Query);
        string sql = result.ToString();

        DataTable table = this.Connection.Execute(sql);

        IEntity[] entities = this.ParentScope.Mapper.MapToNew(loader.EntityMetadata, table);
        return new EntityLoadResult(entities);
    }

    public ILoadResult<DataRow> LoadRaw(IQueryLoader loader)
    {
        this.CheckConnection();

        var compiler = new PostgresCompiler();
        SqlResult result = compiler.Compile(loader.Query);
        string sql = result.ToString();

        DataTable table = this.Connection.Execute(sql);

        return new LoadResult<DataRow>(table.Rows.Cast<DataRow>());
    }

    protected DbVariable GetDbVariable(IDbFieldMetadata fm, IEntity entity, string parameterPostfix)
    {
        object lowerValue = fm.ValueGetterLower(entity, fm.Name);
        DbVariable dbVariable = PostgreHelper.GetData(fm, lowerValue);
        if (!string.IsNullOrEmpty(parameterPostfix))
            dbVariable.ParameterName += parameterPostfix;

        return dbVariable;
    }

    protected DbVariable[] GetDbVariables(IDbEntityMetadata em, IEntity entity, string parameterPostfix)
    {
        List<DbVariable> dbVariables = new List<DbVariable>();
        foreach (IDbFieldMetadata fm in em.Fields.Values)
        {
            if (!fm.IsPersisted)
                continue;

            DbVariable dbVariable = this.GetDbVariable(fm, entity, parameterPostfix);

            dbVariables.Add(dbVariable);
        }

#if DEBUG
        Log.Verbose(dbVariables.ToLogStr());
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

    protected DbVariable[] GetDbVariables(IDbEntityMetadata em, IPartialEntity entity, string parameterPostfix)
    {
        PartialEntity partialEntity = (PartialEntity)entity;
        ObjDictionary values = partialEntity.GetAllValues();

        List<DbVariable> dbVariables = new List<DbVariable>();

        foreach (var kv in values)
        {
            IDbFieldMetadata fm = em.Fields[kv.Key];
            if (!fm.IsPersisted)
                continue;

            DbVariable dbVariable = this.GetDbVariable(fm, entity, parameterPostfix);
            dbVariables.Add(dbVariable);
        }

#if DEBUG
        Log.Verbose(dbVariables.ToLogStr());
#endif

        return dbVariables.ToArray();
    }

    [Obsolete("Disable for bulk update issue", true)]
    protected IEntityUpdateResult InsertV2(IDbEntityMetadata em, IEnumerable<IEntity> entities)
    {
        this.CheckConnection();

        EntityUpdateResult result = new EntityUpdateResult();

        TemplateInfo info = Database.EntityFactory.GetTemplate(em, this.ParentScope);
        string schemaName = PostgreHelper.CheckObjectName(this.ParentScope.SchemaName);

        int requiredIdCount = entities.Count(ent => (long)ent.GetId() < 1);
        long[] ids = new long[0];
        if (requiredIdCount > 0)
        {
            if (requiredIdCount == 1)
                ids = new long[] { this.ParentScope.Data.Sequence(info.PersistentSequence).GetNext() };
            else
                ids = this.ParentScope.Data.Sequence(info.PersistentSequence).GetNextN(requiredIdCount);
        }

        DataTable variableTable = GetDbVariableTable(schemaName, em, null);

        int idCount = 0;
        foreach (IEntity entity in entities)
        {
            if (entity is IPartialEntity)
                throw new InvalidOperationException($"Partial entities cannot be inserted ('{entity.GetType().Name}')");

            EntityChangeResultItem ures = new EntityChangeResultItem();
            ures.Name = em.Name;
            long oldId = (long)entity.GetId();

            ures.OldId = oldId;
            ures.Id = oldId;

            try
            {
                if (oldId > 0)
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
                row[columnName] = lowerValue ?? DBNull.Value;
            }
            variableTable.Rows.Add(row);
        }

        this.Connection.BulkUpdate(schemaName, variableTable);

        foreach (IEntity entity in entities)
        {
            entity._IsNew = false;
        }

        return result;
    }

    protected IEntityUpdateResult Insert(IDbEntityMetadata em, IEnumerable<IEntity> entities)
    {
        this.CheckConnection();

        EntityUpdateResult result = new EntityUpdateResult();

        TemplateInfo info = Database.EntityFactory.GetTemplate(em, this.ParentScope);

        int requiredIdCount = entities.Count(ent => (long)ent.GetId() < 1);
        long[] ids = new long[0];
        if (requiredIdCount > 0)
        {
            if (requiredIdCount == 1)
                ids = new long[] { this.ParentScope.Data.Sequence(info.PersistentSequence).GetNext() };
            else
                ids = this.ParentScope.Data.Sequence(info.PersistentSequence).GetNextN(requiredIdCount);
        }

        int idCount = 0;
        foreach (IEntity entity in entities)
        {
            if (entity is IPartialEntity)
                throw new InvalidOperationException($"Partial entities cannot be inserted ('{entity.GetType().Name}')");

            EntityChangeResultItem ures = new EntityChangeResultItem();
            ures.Name = em.Name;
            long oldId = (long)entity.GetId();

            ures.OldId = oldId;
            ures.Id = oldId;

            try
            {
                if (oldId > 0)
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

            List<DbVariable[]> fields = new List<DbVariable[]>();
            List<DbVariable> flatFields = new List<DbVariable>();

            for (int i = 0; i < count; i++)
            {
                IEntity entity = entities.ElementAt(i);
                DbVariable[] variables = this.GetDbVariables(em, entity, i.ToString("0000"));
                fields.Add(variables);
                flatFields.AddRange(variables);
            }



            string sql = this.DdlGenerator.Insert(this.ParentScope.SchemaName, em.TableName, majorVariables, fields);

            this.Connection.Execute(sql, flatFields.ToArray());
        }
        else
        {
            IEntity entity = entities.First();
            DbVariable[] variables = this.GetDbVariables(em, entity, null);

            string sql = this.DdlGenerator.Insert(this.ParentScope.SchemaName, em.TableName, variables);
            this.Connection.Execute(sql, variables);
        }

        foreach (IEntity entity in entities)
        {
            entity._IsNew = false;
        }

        return result;
    }

    protected void Update(IDbEntityMetadata em, IEntity entity)
    {
        this.CheckConnection();

        DbVariable[] variables;

        if (entity is IPartialEntity)
            variables = this.GetDbVariables(em, (IPartialEntity)entity, null);
        else
            variables = this.GetDbVariables(em, entity, null);

        DbVariable idVariable = this.GetDbVariable(em.PrimaryKey, entity, null);

        string sql = this.DdlGenerator.Update(this.ParentScope.SchemaName, em, idVariable, variables);
        DataTable table = this.Connection.Execute(sql, variables);
        int updatedRecordCount = 0;
        updatedRecordCount = table.Rows[0].To<int>(0);

        try
        {
            updatedRecordCount.Should().Be(1, $"Entity '{entity}' can't update. Possible not found with id: {entity.GetId()}");
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    protected IEntityUpdateResult Update(IDbEntityMetadata em, IEnumerable<IEntity> entities)
    {
        this.CheckConnection();

        EntityUpdateResult result = new EntityUpdateResult();

        foreach (IEntity entity in entities)
        {
            this.Update(em, entity);
            result.Modified(entity);
        }

        return result;
    }

    public IEntityUpdateResult InsertOrUpdate(IDbEntityMetadata em, IEnumerable<IEntity> entities)
    {
        this.CheckConnection();

        EntityUpdateResult result = new EntityUpdateResult();

        var division = entities.GroupBy(ent => ent._IsNew);

        var newEntities = division.FirstOrDefault(d => d.Key == true);
        var updatedEntities = division.FirstOrDefault(d => d.Key == false);

        if (newEntities != null && newEntities.Any())
        {
            result.MergeWith(this.Insert(em, newEntities));
        }

        if (updatedEntities != null && updatedEntities.Any())
            result.MergeWith(this.Update(em, updatedEntities));

        return result;
    }

    public IIntSequence Sequence(string name)
    {
        this.CheckConnection();

        return new PostgreSqlSequence(this, name);
    }

    protected IEntityUpdateResult _Delete(IDbEntityMetadata em, IEnumerable<long> ids)
    {
        this.CheckConnection();
        EntityUpdateResult result = new EntityUpdateResult();

        string sql = this.DdlGenerator.Delete(this.ParentScope.SchemaName, em, ids);
        this.Connection.Execute(sql);

        foreach (long id in ids)
        {
            result.Deleted(em.Name, id);
        }
        return result;
    }

    public IEntityUpdateResult Delete(IDbEntityMetadata em, IEnumerable<long> ids)
    {
        EntityUpdateResult result = new EntityUpdateResult();
        var parts = ids.Split(10);
        foreach (var part in parts)
            result.MergeWith(
                this._Delete(em, part));

        return result;
    }

    public IEntityUpdateResult BulkUpdate(IDbEntityMetadata em, IQueryUpdater query)
    {
        this.CheckConnection();

        SqlKata.Query _updateQuery = query.Query.Clone();
        _updateQuery.ClearComponent("select");
        _updateQuery.AsUpdate(query.UpdateData);

        var compiler = new PostgresCompiler();
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
        DataTable table = this.Connection.Execute(sql);

        EntityUpdateResult uResult = new EntityUpdateResult();
        foreach (DataRow row in table.Rows)
        {
            long id = row.To<long>("Id");
            uResult.Modified(id);
        }

        return uResult;
    }
}
