using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Rapidex.Data.PostgreServer;

internal static class PostgreDataSourceProvider
{
    private static DictionaryA<ThrottledNpgsqlDataSourceWrapper> DataSources = new();


    public static ThrottledNpgsqlDataSourceWrapper Get(string connectionString)
    {
        var dataSource = DataSources.GetOr(connectionString, () =>
          {
              var dataSource = NpgsqlDataSource.Create(connectionString);
              var throttledDataSource = new ThrottledNpgsqlDataSourceWrapper(dataSource);
              return throttledDataSource;
          });

        return dataSource;
    }

}
