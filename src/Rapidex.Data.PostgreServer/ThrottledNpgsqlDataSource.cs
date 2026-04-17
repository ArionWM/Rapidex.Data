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
        bool acquired = await this.lockObject.WaitAsync(TimeSpan.FromSeconds(60));
        if (!acquired)
            throw new TimeoutException($"Database connection semaphore could not be acquired within 30 seconds.");
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
        try
        {
            await this.AccuireLock();
            return await this.dataSource.NotNull().OpenConnectionAsync();
        }
        catch (TimeoutException)
        {
            throw;
        }
        catch
        {
            this.ReleaseLock();
            throw;
        }
    }

}
