using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.PostgreServer;

internal class PostgreSqlServerNamingHelper : IDbNamingHelper
{
    public string CheckColumnName(string name)
    {
        return PostgreHelper.CheckObjectName(name);
    }

    public string CheckDatabaseName(string name)
    {
        return PostgreHelper.CheckObjectName(name);
    }

    public string CheckTableName(string name)
    {
        return PostgreHelper.CheckObjectName(name);
    }
}
