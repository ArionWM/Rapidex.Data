using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.SqlServer;

internal class DbSqlServerNamingHelper : IDbNamingHelper
{
    public string CheckColumnName(string name)
    {
        return name.ClearSpecials();
    }

    public string CheckDatabaseName(string name)
    {
        return name.ClearSpecials();
    }

    public string CheckTableName(string name)
    {
        return name.ClearSpecials();
    }
}
