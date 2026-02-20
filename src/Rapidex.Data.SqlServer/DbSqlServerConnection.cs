using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Rapidex.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Rapidex.Data.SqlServer;

internal class DbSqlServerConnection : IDisposable //TODO: convert to DI + initialize 
{
    protected int DebugId { get; }
    protected string ConnectionString { get; }
    internal SqlConnection Connection { get; private set; }
    internal SqlTransaction Transaction { get; private set; }

    public bool IsTransactionAvailable { get { return this.Transaction != null; } }

    public DbSqlServerConnection(string connectionString)
    {
        this.DebugId = RandomHelper.Random(99999999);
        this.ConnectionString = connectionString;
        this.Connection = new SqlConnection(connectionString);
        this.Connection.OpenAsync();
    }

    protected void CheckConnectionState()
    {
        this.ConnectionString.NotEmpty("ConnectionString can't empty");
        this.Connection.NotNull("Connection instance is null");

        int retryCount = 0;
        while (this.Connection.State == ConnectionState.Connecting)
        {
            // wait for connection to be established
            Thread.Sleep(50);

            retryCount++;

            if (retryCount > 500)
            {
                Common.DefaultLogger?.LogError("Connection is not established after 30 second. ConnectionString: " + this.ConnectionString);
                throw new InvalidOperationException("SQL Server Connection is not established after 30 second. Check SQL server accessibility."); //TODO: DatabaseConnectionException
            }
        }

        switch (this.Connection.State)
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

    public SqlCommand CreateCommand()
    {
        SqlCommand command = this.Connection.CreateCommand();
        command.Transaction = this.Transaction;
        command.CommandTimeout = 60;
        return command;
    }

    public SqlCommand CreateCommand(DbVariable[] parameters)
    {

        SqlCommand command = this.CreateCommand();

        for (int i = 0; i < parameters.Length; i++)
        {
            DbVariable col = parameters[i];
            SqlParameter parameter = command.Parameters.Add(col.ParameterName, DbSqlServerHelper.Convert(col.DbType));
            parameter.Value = col.Value;
        }

        return command;
    }

    public DataTable Execute(string sql, params DbVariable[] parameters)
    {
        this.CheckConnectionState();
        using (SqlCommand command = this.CreateCommand(parameters))
            try
            {
#if DEBUG                    
                Stopwatch sw = Stopwatch.StartNew();
                string logLine = DbSqlServerHelper.CreateSqlLog(this.DebugId, sql, parameters);
                Common.DefaultLogger?.LogDebug(logLine);
#endif         

                command.CommandText = sql;
                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.Default)) //Ne zaman CommandBehavior.SequentialAccess kullanalım?
                {
                    DataTable table = new DataTable();
                    table.Load(reader);

#if DEBUG
                    sw.Stop();
                    Common.DefaultLogger?.LogDebug($"({this.DebugId}) {table.Rows.Count} row(s) returned (at {sw.ElapsedMilliseconds:#,#} ms {(sw.ElapsedMilliseconds > 500 ? "*" : "")}{(sw.ElapsedMilliseconds > 100 ? "*" : "")})");
#endif

                    return table;
                }
            }
            catch (Exception ex)
            {
                string logLine = DbSqlServerHelper.CreateSqlLog(this.DebugId, sql, parameters);
                Common.DefaultLogger?.LogError($"({this.DebugId}) {ex.Message}\r\n{logLine}");
                Common.DefaultLogger?.LogWarning($"({this.DebugId}) \r\n" + Environment.StackTrace);
                var tex = DbSqlServerProvider.SqlServerExceptionTranslator.Translate(ex, "See details in error logs; \r\n" + sql) ?? ex;
                tex.Log();
                throw tex;
            }
    }


    public DataTable Execute(string sql, DataTable variableTable)
    {
        this.CheckConnectionState();
        using (SqlCommand command = this.CreateCommand())
            try
            {
                var param = command.Parameters.Add("@table", SqlDbType.Structured);
                param.Value = variableTable;
                param.TypeName = variableTable.TableName;

                command.CommandType = CommandType.Text;
                command.CommandText = sql;
                command.ExecuteNonQuery();
                return null;
            }
            catch (Exception ex)
            {
                Common.DefaultLogger?.LogWarning($"({this.DebugId}) \r\n" + Environment.StackTrace);
                Common.DefaultLogger?.LogError($"{ex.Message}\r\n{sql}");

                var tex = DbSqlServerProvider.SqlServerExceptionTranslator.Translate(ex, sql + " / " + variableTable.TableName) ?? ex;
                tex.Log();
                throw tex;
            }
    }
    public void Dispose()
    {
        if (this.Connection != null)
        {
            Common.DefaultLogger?.LogDebug($"Connection [{Thread.CurrentThread.ManagedThreadId} / {this.Connection.ClientConnectionId}]: closed.");

            this.Connection.Close();
            this.Connection.Dispose();
            this.Connection = null;
        }
    }
}
