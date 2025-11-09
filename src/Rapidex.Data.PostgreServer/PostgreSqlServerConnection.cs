using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Npgsql;
using Rapidex.Data.Helpers;

namespace Rapidex.Data.PostgreServer;

internal class PostgreSqlServerConnection : IDisposable
{
    protected object lockObject = new object();

    protected int DebugId { get; }
    protected string ConnectionString { get; }
    internal NpgsqlConnection Connection { get; private set; }
    internal NpgsqlTransaction Transaction { get; private set; }

    public bool IsTransactionAvailable => this.Transaction != null;

    public PostgreSqlServerConnection(string connectionString)
    {
        this.DebugId = RandomHelper.Random(99999999);

        NpgsqlConnectionStringBuilder npgsqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        if (npgsqlConnectionStringBuilder.Timeout < 30)
            npgsqlConnectionStringBuilder.Timeout = 30;

        npgsqlConnectionStringBuilder.CommandTimeout = 60;
        if (npgsqlConnectionStringBuilder.MaxPoolSize < 200)
            npgsqlConnectionStringBuilder.MaxPoolSize = 200;

        //npgsqlConnectionStringBuilder.Database = PostgreHelper.CheckObjectName(npgsqlConnectionStringBuilder.Database);

        this.ConnectionString = npgsqlConnectionStringBuilder.ConnectionString;
        this.Connection = new NpgsqlConnection(this.ConnectionString);
        this.Connection.Open();

        Log.Debug("Database", "Connection [{0}, {1}, {2}]: opened.", this.DebugId, Thread.CurrentThread.ManagedThreadId, this.Connection.ProcessID);
    }

    protected void CheckConnectionState()
    {
        this.ConnectionString.NotEmpty("ConnectionString can't be empty");

        int retryCount = 0;
        while (this.Connection.FullState.HasFlag(ConnectionState.Connecting))
        {
            Thread.Sleep(50);
            retryCount++;
            if (retryCount > 200)
            {
                Log.Error("Database", $"Connection is not established after 10 seconds. {this.DebugId}; ConnectionString: {this.ConnectionString}");
                throw new InvalidOperationException("Connection is not established after 10 seconds.");
            }
        }

        switch (this.Connection.FullState)
        {
            case ConnectionState.Closed:
            case ConnectionState.Broken:
                this.Connection.Open();
                break;
        }
    }

    public void BeginTransaction()
    {
        this.CheckConnectionState();
        this.Transaction = this.Connection.BeginTransaction();
    }

    public NpgsqlCommand CreateCommand()
    {
        var command = this.Connection.CreateCommand();
        command.Transaction = this.Transaction;
        //command.CommandTimeout = this.CommandTimeout;
        return command;
    }

    public NpgsqlCommand CreateCommand(DbVariable[] parameters)
    {
        var command = this.CreateCommand();

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
            this.CheckConnectionState();
            using (var command = this.CreateCommand(parameters))
            {
                try
                {
                    if (Log.IsDebugEnabled)
                    {
                        string logLine = LogHelper.CreateSqlLog(this.DebugId, sql, parameters);
                        Log.Debug("Database", logLine);
                    }

                    command.CommandText = sql;
                    using (var reader = command.ExecuteReader(CommandBehavior.Default))
                    {
                        DataTable table = new DataTable();
                        table.Load(reader);

                        if (Log.IsDebugEnabled)
                            Log.Debug("Database", $"{table.Rows.Count} row(s) returned");

                        table.CaseSensitive = false;
                        return table;
                    }
                }
                catch (Exception ex)
                {
                    string logLine = LogHelper.CreateSqlLog(this.DebugId, sql, parameters);
                    Log.Error("Database", $"{this.DebugId}; {ex.Message}\r\n{logLine}");
                    Log.Warn("Database", Environment.StackTrace);
                    var tex = PostgreSqlServerProvider.PostgreServerExceptionTranslator.Translate(ex, "See details in error logs; \r\n" + sql) ?? ex;
                    tex.Log();
                    throw tex;
                }
            }
        }
    }

    public void BulkUpdate(string schemaName, DataTable variableTable)
    {
        lock (this.lockObject) //PostgreSQL NpgsqlConnection is not support MARS
        {
            this.CheckConnectionState();

            try
            {
                //See: https://gist.github.com/samlii/a646660ced448fa1d8dd6642da358f3e
                //See: https://www.bytefish.de/blog/postgresql_bulk_insert.html
                this.Connection.BulkUpdate(schemaName, variableTable);
            }
            catch (Exception ex)
            {
                Log.Warn("Database", Environment.StackTrace);
                Log.Error("Database", $"{this.DebugId}; {ex.Message}\r\n{variableTable.TableName}");

                var tex = PostgreSqlServerProvider.PostgreServerExceptionTranslator.Translate(ex, schemaName + ", " + variableTable.TableName) ?? ex;
                tex.Log();
                throw tex;
            }
        }
    }

    public void Dispose()
    {
        //TODO: Check https://github.com/npgsql/npgsql/issues/1201 NpgsqlConnection.ClearPool(NpgsqlConnection) ?

        Log.Debug("Database", "Connection [{0}, {1}, {2}]: closed.", this.DebugId, Thread.CurrentThread.ManagedThreadId, this.Connection.ProcessID);
        if (this.Connection != null)
        {
            this.Connection.Close();
            this.Connection.Dispose();
            this.Connection = null;
        }
    }
}
