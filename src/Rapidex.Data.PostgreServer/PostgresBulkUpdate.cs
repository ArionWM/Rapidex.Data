using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.PostgreServer;

public static class PostgresBulkUpdate
{
    //https://gist.github.com/samlii/a646660ced448fa1d8dd6642da358f3e
    public static async Task BulkUpdate(this NpgsqlConnection connection, string schemaName, DataTable dataTable,
        Dictionary<string, string> columnMap = null, Dictionary<Type, NpgsqlDbType> columnTypes = null)
    {
        if (dataTable.PrimaryKey == null || dataTable.PrimaryKey.Length == 0)
            throw new ArgumentException("No primary keys specified", nameof(dataTable));

        var wasClosed = connection.State != ConnectionState.Open;
        if (wasClosed)
            connection.Open();

        columnMap = columnMap ?? new Dictionary<string, string>();
        columnTypes = columnTypes ?? new Dictionary<Type, NpgsqlDbType>();

        var tableName = dataTable.TableName;
        var columnNames = (from DataColumn column in dataTable.Columns
                           select columnMap.ContainsKey(column.ColumnName) ? columnMap[column.ColumnName] : column.ColumnName);

        columnNames = columnNames.Select(cn => "\"" + cn + "\"");

        var allColumns = string.Join(",", columnNames);
        try
        {

            using (var trans = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted))
            {

                await connection.Execute($"CREATE TEMP TABLE {tableName}_tmp ON COMMIT DROP AS SELECT {allColumns} FROM {schemaName}.{tableName} LIMIT 0 ;");

                using (
                    var writer =
                        await connection.BeginBinaryImportAsync($"COPY {tableName}_tmp({allColumns}) FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        await writer.StartRowAsync();
                        foreach (var item in row.ItemArray)
                        {
                            if (columnTypes.ContainsKey(item.GetType()))
                            {
                                if (columnTypes[item.GetType()] == NpgsqlDbType.Json)
                                {
                                    writer.Write(item.ToJson(), NpgsqlDbType.Json);
                                }
                                else
                                {
                                    writer.Write(item, columnTypes[item.GetType()]);
                                }
                            }
                            else
                            {
                                writer.Write(item);
                            }
                        }
                    }
                }

                var newColumns = string.Join(",", columnNames.Select(c => $"new.{c}"));

                var whereClause = string.Join(" AND ",
                    dataTable.PrimaryKey.Select(c => $"orig.{c.ColumnName} = new.{c.ColumnName}"));

                await connection.Execute(
                     $"LOCK TABLE {schemaName}.{tableName} IN EXCLUSIVE MODE");


                await connection.Execute(
                    $"UPDATE {schemaName}.{tableName} orig SET ({allColumns}) = ({newColumns}) FROM {tableName}_tmp \"new\" WHERE {whereClause}");

                await connection.Execute(
                    $@"INSERT INTO {schemaName}.{tableName} ({allColumns})
                                          SELECT {newColumns}
                                          FROM {tableName}_tmp ""new""
                                          WHERE NOT EXISTS (
                                             SELECT NULL
                                             FROM {tableName} orig
                                             WHERE {whereClause})");
                trans.Commit();
            }
        }
        finally
        {
            await connection.Execute($"DROP TABLE {tableName}_tmp");
            if (wasClosed)
                connection.Close();
        }

    }

}
