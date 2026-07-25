using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Polly;
using Rapidex.Data.Helpers;

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


    protected async Task<DataTable> ExecuteInternal(ResilienceContext context, string sql, params DbVariable[] parameters)
    {
        int trackId = RandomHelper.Random(999999);
        this.CheckConnectionState();
        using (SqlCommand command = this.CreateCommand(parameters))
            try
            {
#if DEBUG                    
                Stopwatch sw = Stopwatch.StartNew();

                string sqlLogLine = DbSqlServerHelper.CreateSqlLog(this.DebugId, trackId, sql, parameters);
                Common.DefaultLogger?.LogDebug(sqlLogLine);
#endif         

                command.CommandText = sql;
                using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.Default)) //Ne zaman CommandBehavior.SequentialAccess kullanalım?
                {
                    DataTable table = new DataTable();
                    table.Load(reader);

#if DEBUG
                    sw.Stop();
                    int slow = (sw.ElapsedMilliseconds > 1000 ? 2 : (sw.ElapsedMilliseconds > 100 ? 1 : 0));
                    string desc = $"({this.DebugId}, {trackId}) {table.Rows.Count} row(s) returned (at {sw.ElapsedMilliseconds:#,#} ms {(sw.ElapsedMilliseconds > 500 ? "*" : "")}{(sw.ElapsedMilliseconds > 100 ? "*" : "")})";

                    switch (slow)
                    {
                        case 2:
                            Common.DefaultLogger?.LogWarning(desc + "\r\n" + sqlLogLine);
                            break;
                        case 1:
                            Common.DefaultLogger?.LogInformation(desc + "\r\n" + sqlLogLine);
                            break;
                        case 0:
                            Common.DefaultLogger?.LogDebug(desc);
                            break;
                    }

#endif

                    return table;
                }
            }
            catch (Exception ex)
            {
                this.LogQueryError(context, sql, trackId, ex, parameters);
                throw;
            }
    }

    /// <summary>
    /// Sorgu hatasını yeniden deneme bağlamıyla birlikte loglar
    /// </summary>
    /// <param name="context">Polly yeniden deneme bağlamı</param>
    /// <param name="sql">Çalıştırılan sorgu</param>
    /// <param name="trackId">Sorgu izleme numarası</param>
    /// <param name="ex">Oluşan hata</param>
    /// <param name="parameters">Sorgu parametreleri</param>
    void LogQueryError(ResilienceContext context, string sql, int trackId, Exception ex, DbVariable[] parameters)
    {
        string logLine = DbSqlServerHelper.CreateSqlLog(this.DebugId, trackId, sql, parameters);

        var propKey = new ResiliencePropertyKey<string>("CorrelationId");
        context.Properties.TryGetValue<string>(propKey, out string correlationId);

        var propKey2 = new ResiliencePropertyKey<int>("AttemptNumber");
        context.Properties.TryGetValue<int>(propKey2, out int attemptNumber);

        var propKey3 = new ResiliencePropertyKey<int>("MaxRetryCount");
        context.Properties.TryGetValue<int>(propKey3, out int maxRetryCount);

        string htext = $"({this.DebugId}) CorrelationId: {correlationId}, Retry: {attemptNumber}/{maxRetryCount} \r\n";
        Common.DefaultLogger?.LogError($"{htext}\r\n{ex.Message}\r\n{logLine}\r\n{sql}\r\n{Environment.StackTrace}");
    }

    /// <summary>
    /// Sorguyu çalıştırır ve sonucu ara bir DataTable üretmeden reader üzerinden okutur
    /// </summary>
    /// <param name="context">Polly yeniden deneme bağlamı</param>
    /// <param name="sql">Çalıştırılacak sorgu</param>
    /// <param name="read">Reader'ı tüketip sonucu üreten işlem</param>
    /// <param name="parameters">Sorgu parametreleri</param>
    protected async Task<T> ExecuteReaderInternal<T>(ResilienceContext context, string sql, Func<DbDataReader, T> read, params DbVariable[] parameters)
    {
        int trackId = RandomHelper.Random(999999);
        this.CheckConnectionState();
        using (SqlCommand command = this.CreateCommand(parameters))
            try
            {
#if DEBUG
                Stopwatch sw = Stopwatch.StartNew();

                string sqlLogLine = DbSqlServerHelper.CreateSqlLog(this.DebugId, trackId, sql, parameters);
                Common.DefaultLogger?.LogDebug(sqlLogLine);
#endif

                command.CommandText = sql;
                using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.Default))
                {
                    T result = read(reader);

#if DEBUG
                    sw.Stop();
                    int slow = (sw.ElapsedMilliseconds > 1000 ? 2 : (sw.ElapsedMilliseconds > 100 ? 1 : 0));
                    string desc = $"({this.DebugId}, {trackId}) read and mapped (at {sw.ElapsedMilliseconds:#,#} ms {(sw.ElapsedMilliseconds > 500 ? "*" : "")}{(sw.ElapsedMilliseconds > 100 ? "*" : "")})";

                    switch (slow)
                    {
                        case 2:
                            Common.DefaultLogger?.LogWarning(desc + "\r\n" + sqlLogLine);
                            break;
                        case 1:
                            Common.DefaultLogger?.LogInformation(desc + "\r\n" + sqlLogLine);
                            break;
                        case 0:
                            Common.DefaultLogger?.LogDebug(desc);
                            break;
                    }
#endif

                    return result;
                }
            }
            catch (Exception ex)
            {
                this.LogQueryError(context, sql, trackId, ex, parameters);
                throw;
            }
    }

    /// <summary>
    /// Sorguyu çalıştırır ve sonucu reader üzerinden okutur. <paramref name="read"/> yeniden
    /// denemede tekrar çağrılabileceği için yan etkisiz olmalıdır.
    /// </summary>
    /// <param name="sql">Çalıştırılacak sorgu</param>
    /// <param name="read">Reader'ı tüketip sonucu üreten işlem</param>
    /// <param name="parameters">Sorgu parametreleri</param>
    public async Task<T> ExecuteReader<T>(string sql, Func<DbDataReader, T> read, params DbVariable[] parameters)
    {
        try
        {
            ResilienceContext context = ResilienceContextPool.Shared.Get(CancellationToken.None);
            var propKey = new ResiliencePropertyKey<string>("CorrelationId");
            context.Properties.Set(propKey, Guid.NewGuid().ToString());

            return await Policies.RetryPipeline.ExecuteAsync(async context => await this.ExecuteReaderInternal(context, sql, read, parameters), context);
        }
        catch (Exception ex)
        {
            string logLine = DbSqlServerHelper.CreateSqlLog(this.DebugId, 0, sql, parameters);
            Common.DefaultLogger?.LogError($"({this.DebugId}) {ex.Message}\r\n{logLine}");
            Common.DefaultLogger?.LogWarning($"({this.DebugId}) \r\n" + Environment.StackTrace);

            //Veritabanı kaynaklı olmayan hatalar (örn; mapper) olduğu gibi, yığın izi korunarak yükselir
            var tex = DbSqlServerProvider.SqlServerExceptionTranslator.Translate(ex, "See details in error logs; \r\n" + sql);
            if (tex == null)
                throw;

            tex.Log();
            throw tex;
        }
    }

    public async Task<DataTable> Execute(string sql, params DbVariable[] parameters)
    {
        try
        {
            ResilienceContext context = ResilienceContextPool.Shared.Get(CancellationToken.None);
            var propKey = new ResiliencePropertyKey<string>("CorrelationId");
            context.Properties.Set(propKey, Guid.NewGuid().ToString());

            var result = await Policies.RetryPipeline.ExecuteAsync(async context => await this.ExecuteInternal(context, sql, parameters), context);
            return result;
        }
        catch (Exception ex)
        {
            string logLine = DbSqlServerHelper.CreateSqlLog(this.DebugId, 0, sql, parameters);
            Common.DefaultLogger?.LogError($"({this.DebugId}) {ex.Message}\r\n{logLine}");
            Common.DefaultLogger?.LogWarning($"({this.DebugId}) \r\n" + Environment.StackTrace);
            var tex = DbSqlServerProvider.SqlServerExceptionTranslator.Translate(ex, "See details in error logs; \r\n" + sql) ?? ex;
            tex.Log();
            throw tex;
        }
    }

    protected async Task<DataTable> ExecuteInternal(ResilienceContext context, string sql, DataTable variableTable)
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
                await command.ExecuteNonQueryAsync();
                return null;
            }
            catch (Exception ex)
            {
                var propKey = new ResiliencePropertyKey<string>("CorrelationId");
                context.Properties.TryGetValue<string>(propKey, out string correlationId);

                var propKey2 = new ResiliencePropertyKey<int>("AttemptNumber");
                context.Properties.TryGetValue<int>(propKey2, out int attemptNumber);

                var propKey3 = new ResiliencePropertyKey<int>("MaxRetryCount");
                context.Properties.TryGetValue<int>(propKey3, out int maxRetryCount);

                string htext = $"({this.DebugId}) CorrelationId: {correlationId}, Retry: {attemptNumber}/{maxRetryCount} \r\n";
                Common.DefaultLogger?.LogError($"{htext}\r\n{ex.Message}\r\n{sql}\r\n{Environment.StackTrace}");
                throw;
            }
    }


    public async Task<DataTable> Execute(string sql, DataTable variableTable)
    {
        try
        {
            ResilienceContext context = ResilienceContextPool.Shared.Get(CancellationToken.None);
            var propKey = new ResiliencePropertyKey<string>("CorrelationId");
            context.Properties.Set(propKey, Guid.NewGuid().ToString());

            var result = await Policies.RetryPipeline.ExecuteAsync(async context => await this.ExecuteInternal(context, sql, variableTable), context);
            return result;
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
