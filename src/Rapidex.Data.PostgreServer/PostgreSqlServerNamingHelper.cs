using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.PostgreServer;

internal class PostgreSqlServerNamingHelper : IDbNamingHelper
{
    static Dictionary<string, string> cache = new Dictionary<string, string>();

    public string CheckColumnName(string name)
    {
        if (cache.TryGetValue(name, out string cachedName))
        {
            return cachedName;
        }

        string checkedName = PostgreHelper.CheckObjectName(name);
        cache[name] = checkedName;
        return checkedName;
    }

    public string CheckDatabaseName(string name)
    {
        if (cache.TryGetValue(name, out string cachedName))
        {
            return cachedName;
        }

        string checkedName = PostgreHelper.CheckObjectName(name);
        cache[name] = checkedName;
        return checkedName;
    }

    public string CheckTableName(string name)
    {
        if (cache.TryGetValue(name, out string cachedName))
        {
            return cachedName;
        }

        string checkedName = PostgreHelper.CheckObjectName(name);
        cache[name] = checkedName;
        return checkedName;
    }
}
