using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using Rapidex.Data.Helpers;

namespace Rapidex.Data.PostgreServer;

internal class PostgreSqlServerConnection : IDisposable //TODO: convert to DI + initialize 
{
    protected object lockObject = new object();
    private NpgsqlConnection connectionWithTransaction;

    protected int DebugId { get; }
    protected string ConnectionString { get; }

    internal NpgsqlDataSource DataSource { get; private set; }
    internal NpgsqlConnection ConnectionWithTransaction { get => connectionWithTransaction; private set => connectionWithTransaction = value; }
    internal NpgsqlTransaction Transaction { get; private set; }

    public bool IsTransactionAvailable => this.Transaction != null;

    public PostgreSqlServerConnection(string connectionString)
    {
        this.DebugId = RandomHelper.Random(99999999);

        NpgsqlConnectionStringBuilder npgsqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        if (npgsqlConnectionStringBuilder.Timeout < 30)
            npgsqlConnectionStringBuilder.Timeout = 30;

        npgsqlConnectionStringBuilder.CommandTimeout = 60;
        if (npgsqlConnectionStringBuilder.MaxPoolSize < 5)
            npgsqlConnectionStringBuilder.MaxPoolSize = 5;


        this.ConnectionString = npgsqlConnectionStringBuilder.ConnectionString;
        this.ConnectionString.NotEmpty("ConnectionString can't be empty");
        if (this.DataSource == null)
            this.DataSource = PostgreDataSourceProvider.Get(this.ConnectionString);

        //this.Connection = new NpgsqlConnection(this.ConnectionString);
        //this.Connection.Open();
        //Common.DefaultLogger?.LogDebug("Database", "Connection [{0}, {1}, {2}]: opened.", this.DebugId, Thread.CurrentThread.ManagedThreadId, this.Connection.ProcessID);
    }

    protected async Task<NpgsqlConnection> GetNewConnection()
    {
        var connection = await this.DataSource.OpenConnectionAsync();

        int retryCount = 0;
        while (connection.FullState.HasFlag(ConnectionState.Connecting))
        {
            await Task.Delay(50);
            retryCount++;
            if (retryCount > 200)
            {
                Common.DefaultLogger?.LogError("Database", $"Connection is not established after 10 seconds. {this.DebugId}; ConnectionString: {this.ConnectionString}");
                throw new InvalidOperationException("Connection is not established after 10 seconds.");
            }
        }

        switch (connection.FullState)
        {
            case ConnectionState.Closed:
            case ConnectionState.Broken:
                connection.Open();
                break;
        }

        return connection;
    }

    protected void CloseConnection(ref NpgsqlConnection connection)
    {
        if (connection != null)
        {
            connection.Close();
            connection.Dispose();
            connection = null;
        }
    }

    public void BeginTransaction()
    {
        var connectionT = this.GetNewConnection();
        this.ConnectionWithTransaction = connectionT.Result;
        this.Transaction = this.ConnectionWithTransaction.BeginTransaction();
    }

    public void CommitTransaction()
    {
        if (this.Transaction != null)
        {
            this.Transaction.Commit();
            this.Transaction.Dispose();
            this.Transaction = null;
        }
        if (this.ConnectionWithTransaction != null)
        {
            this.CloseConnection(ref this.connectionWithTransaction);
        }
    }

    public void RollbackTransaction()
    {
        try
        {
            if (this.Transaction != null)
            {
                this.Transaction.Rollback();
                this.Transaction.Dispose();
                this.Transaction = null;
            }
        }
        finally
        {
            if (this.ConnectionWithTransaction != null)
            {
                this.CloseConnection(ref this.connectionWithTransaction);
            }
        }
    }

    public NpgsqlCommand CreateCommand(NpgsqlConnection connection)
    {
        var command = connection.CreateCommand();
        command.Transaction = this.Transaction;
        return command;
    }

    public NpgsqlCommand CreateCommand(NpgsqlConnection connection, DbVariable[] parameters)
    {
        var command = this.CreateCommand(connection);

        for (int i = 0; i < parameters.Length; i++)
        {
            DbVariable col = parameters[i];
            var parameter = command.Parameters.Add(col.ParameterName, PostgreHelper.Convert(col.DbType));
            parameter.Value = PostgreHelper.CheckValue(col.Value);
        }

        return command;
    }

    public DataTable Execute(string sql, params DbVariable[] parameters)
    {
        lock (this.lockObject) //PostgreSQL NpgsqlConnection is not support MARS
        {
            NpgsqlConnection connection = null;
            if (this.IsTransactionAvailable)
                connection = this.ConnectionWithTransaction;
            else
                connection = this.GetNewConnection().Result;

            try
            {

                using (var command = this.CreateCommand(connection, parameters))
                {
                    try
                    {
#if DEBUG
                        string logLine = PostgreHelper.CreateSqlLog(this.DebugId, sql, parameters);
                        Common.DefaultLogger?.LogDebug("Database", logLine);
#endif

                        command.CommandText = sql;
                        using (var reader = command.ExecuteReader(CommandBehavior.Default))
                        {
                            DataTable table = new DataTable();
                            table.Load(reader);

#if DEBUG
                            Common.DefaultLogger?.LogDebug("Database", $"({this.DebugId}) {table.Rows.Count} row(s) returned");
#endif

                            table.CaseSensitive = false;
                            return table;
                        }
                    }
                    catch (Exception ex)
                    {
                        string logLine = PostgreHelper.CreateSqlLog(this.DebugId, sql, parameters);
                        Common.DefaultLogger?.LogError("Database", $"({this.DebugId}) {ex.Message}\r\n{logLine}");
                        Common.DefaultLogger?.LogWarning("Database", $"({this.DebugId}) \r\n" + Environment.StackTrace);
                        var tex = PostgreSqlServerProvider.PostgreServerExceptionTranslator.Translate(ex, "See details in error logs; \r\n" + sql) ?? ex;
                        tex.Log();
                        throw tex;
                    }
                }
            }
            finally
            {
                if (!this.IsTransactionAvailable && connection != null)
                {
                    this.CloseConnection(ref connection);
                }
            }
        }
    }

    public void BulkUpdate(string schemaName, DataTable variableTable)
    {
        lock (this.lockObject) //PostgreSQL NpgsqlConnection is not support MARS
        {
            NpgsqlConnection connection = null;
            if (this.IsTransactionAvailable)
                connection = this.ConnectionWithTransaction;
            else
                connection = this.GetNewConnection().Result;

            try
            {
                //See: https://gist.github.com/samlii/a646660ced448fa1d8dd6642da358f3e
                //See: https://www.bytefish.de/blog/postgresql_bulk_insert.html
                connection.BulkUpdate(schemaName, variableTable);
            }
            catch (Exception ex)
            {
                Common.DefaultLogger?.LogWarning("Database", $"({this.DebugId}) \r\n" + Environment.StackTrace);
                Common.DefaultLogger?.LogError("Database", $"{this.DebugId}; {ex.Message}\r\n{variableTable.TableName}");

                var tex = PostgreSqlServerProvider.PostgreServerExceptionTranslator.Translate(ex, schemaName + ", " + variableTable.TableName) ?? ex;
                tex.Log();
                throw tex;
            }
            finally
            {
                if (!this.IsTransactionAvailable && connection != null)
                {
                    this.CloseConnection(ref connection);
                }
            }
        }
    }

    public void Dispose()
    {
        //TODO: Check https://github.com/npgsql/npgsql/issues/1201 NpgsqlConnection.ClearPool(NpgsqlConnection) ?

        //Common.DefaultLogger?.LogDebug("Database", "Connection [{0}, {1}, {2}]: closed.", this.DebugId, Thread.CurrentThread.ManagedThreadId, this.Connection.ProcessID);
        if (this.connectionWithTransaction != null)
        {
            this.CloseConnection(ref this.connectionWithTransaction);
        }
    }
}
