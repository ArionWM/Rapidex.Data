using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace Rapidex.Data.PostgreServer;

internal static class PostgreDataSourceProvider
{
    public static DictionaryA<NpgsqlDataSource> dataSources = new();


    public static NpgsqlDataSource Get(string connectionString)
    {
        var dataSource = dataSources.GetOr(connectionString, () =>
          {
              var dataSource = NpgsqlDataSource.Create(connectionString);
              return dataSource;
          });

        return dataSource;
    }

}
