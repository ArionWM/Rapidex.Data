using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;

namespace Rapidex.Data.SqlServer;

internal class DbSqlServerConnection : IDisposable
{
    protected string ConnectionString { get; }
    internal SqlConnection Connection { get; private set; }
    internal SqlTransaction Transaction { get; private set; }

    public bool IsTransactionAvailable { get { return this.Transaction != null; } }

    public DbSqlServerConnection(string connectionString)
    {
        this.ConnectionString = connectionString;
        this.Connection = new SqlConnection(connectionString);
        this.Connection.OpenAsync();
    }

    protected void CheckConnectionState()
    {
        this.ConnectionString.NotEmpty("ConnectionString can't empty");

        int retryCount = 0;
        while (this.Connection.State == ConnectionState.Connecting)
        {
            // wait for connection to be established
            Thread.Sleep(50);

            retryCount++;

            if (retryCount > 200)
            {
                Log.Error("Database", "Connection is not established after 10 second. ConnectionString: " + this.ConnectionString);
                throw new InvalidOperationException("SQL Server Connection is not established after 10 second. Check SQL server accessibility."); //TODO: DatabaseConnectionException
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
        //command.CommandTimeout = this.CommandTimeout;
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

                //if (Log.IsDebugEnabled)
                //    Log.Verbose("Database", this.SqlCommandToRunnableString(context.SQL, context.Parameters));

                command.CommandText = sql;
                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.Default)) //Ne zaman CommandBehavior.SequentialAccess kullanalım?
                {
                    DataTable table = new DataTable();
                    table.Load(reader);

                    if (Log.IsDebugEnabled)
                        Log.Verbose("Database", $"{table.Rows.Count} row(s) returned");

                    return table;
                }
            }
            catch (Exception ex)
            {
                //string sqlLog = this.SqlCommandToRunnableString(context.SQL, context.Parameters);
                //Log.Error("Database", ex, sqlLog);
                Log.Warn("Database", Environment.StackTrace);
                Log.Error("Database", $"{ex.Message}\r\n{sql}");

                var tex = DbSqlServerProvider.SqlServerExceptionTranslator.Translate(ex) ?? ex;
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

                //var param = command.Parameters.AddWithValue("@table", variableTable);
                var param = command.Parameters.Add("@table", SqlDbType.Structured);
                param.Value = variableTable;
                param.TypeName = variableTable.TableName;

                //if (Log.IsDebugEnabled)
                //    Log.Verbose("Database", this.SqlCommandToRunnableString(context.SQL, context.Parameters));

                command.CommandType = CommandType.Text;
                command.CommandText = sql;
                command.ExecuteNonQuery();
                return null;
            }
            catch (Exception ex)
            {
                //string sqlLog = this.SqlCommandToRunnableString(context.SQL, context.Parameters);
                //Log.Error("Database", ex, sqlLog);
                Log.Warn("Database", Environment.StackTrace);
                Log.Error("Database", $"{ex.Message}\r\n{sql}");

                var tex = DbSqlServerProvider.SqlServerExceptionTranslator.Translate(ex) ?? ex;
                tex.Log();
                throw tex;
            }
    }
    void IDisposable.Dispose()
    {
        Log.Debug("Database", $"Connection [{Thread.CurrentThread.ManagedThreadId} / {this.Connection.ClientConnectionId}]: closed.");
        this.Connection.Close();
        this.Connection.Dispose();
    }
}
