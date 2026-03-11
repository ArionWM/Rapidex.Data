using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Rapidex.Data.PostgreServer;

internal class ThrottledNpgsqlDataSourceWrapper
{
    private NpgsqlDataSource dataSource;
    private readonly SemaphoreSlim lockObject;

    public async Task AccuireLock()
    {
        await this.lockObject.WaitAsync(TimeSpan.FromSeconds(30));
    }

    public void ReleaseLock()
    {
        this.lockObject.Release();
    }

    public ThrottledNpgsqlDataSourceWrapper(NpgsqlDataSource source)
    {
        this.dataSource = source;

        NpgsqlConnectionStringBuilder npgsqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder(source.ConnectionString);
        npgsqlConnectionStringBuilder.CheckConnection();

        this.lockObject = new SemaphoreSlim(npgsqlConnectionStringBuilder.MaxPoolSize, npgsqlConnectionStringBuilder.MaxPoolSize);
    }

    public NpgsqlDataSource DataSource { get { return dataSource; } }

    public async ValueTask<NpgsqlConnection> OpenConnectionAsync()
    {
        await this.AccuireLock();
        try
        {
            return await this.dataSource.NotNull().OpenConnectionAsync();
        }
        catch
        {
            this.ReleaseLock();
            throw;
        }
    }

}
