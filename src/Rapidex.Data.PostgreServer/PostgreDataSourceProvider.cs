using System.Collections.Concurrent;
using Npgsql;

namespace Rapidex.Data.PostgreServer;

internal static class PostgreDataSourceProvider
{
    private static readonly ConcurrentDictionary<string, ThrottledNpgsqlDataSourceWrapper> DataSources =
        new(StringComparer.InvariantCultureIgnoreCase);

    public static ThrottledNpgsqlDataSourceWrapper Get(string connectionString)
    {
        return DataSources.GetOrAdd(connectionString, static key =>
        {
            var dataSource = NpgsqlDataSource.Create(key);
            return new ThrottledNpgsqlDataSourceWrapper(dataSource);
        });
    }
}
