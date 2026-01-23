using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Rapidex.Data.SqlServer;

internal class DbSqlSequence : IIntSequence
{
    protected DbSqlServerDataModificationProvider Parent { get; }
    protected DbSqlDdlGenerator DdlGenerator { get; }

    public string DbName { get; set; }
    public string SchemaName { get; set; }
    public string Name { get; set; }

    public long CurrentValue => this.GetCurrentValue();

    public DbSqlSequence(DbSqlServerDataModificationProvider parent, string name)
    {
        this.Parent = parent;
        this.DbName = parent.ParentScope.ParentDbScope.Name;
        this.SchemaName = parent.ParentScope.SchemaName;
        this.Name = name;

        this.DdlGenerator = new DbSqlDdlGenerator();
    }

    protected long GetCurrentValue()
    {
        this.Parent.CheckConnection();

        string sql = this.DdlGenerator.GetCurrentSequenceValue(this.SchemaName, this.Name);
        DataTable table = this.Parent.Connection.Execute(sql);
        return Convert.ToInt64(table.Rows[0][0]);

    }

    public void CreateIfNotExists(long minValue, long currentValue)
    {
        if (minValue < 0)
            minValue = 1;

        if (currentValue < minValue)
            currentValue = minValue;

        string sql = this.DdlGenerator.CreateSequenceIfNotExists(this.SchemaName, this.Name, minValue, currentValue);
        this.Parent.CheckConnection();
        this.Parent.Connection.Execute(sql);
    }

    public long GetNext()
    {
        this.Parent.CheckConnection();

        string sql = this.DdlGenerator.GetNextSequenceValue(this.SchemaName, this.Name);
        DataTable table = this.Parent.Connection.Execute(sql);
        long val = Convert.ToInt64(table.Rows[0][0]);
        return val;
    }

    public long[] GetNextN(int count)
    {
        this.Parent.CheckConnection();

        List<long> values = new List<long>();
        string sql = this.DdlGenerator.GetNextNSequenceValues(this.SchemaName, this.Name, count);
        DataTable table = this.Parent.Connection.Execute(sql);

        var row = table.Rows[0];
        long firstVal = row["FirstVal"].As<long>();
        long lastVal = row["LastVal"].As<long>();

        for (long v = firstVal; v <= lastVal; v++)
        {
            values.Add(v);
        }

        return values.ToArray();
    }

    public long Relocate(long startAt)
    {
        this.Parent.CheckConnection();

        string sql = this.DdlGenerator.RelocateSequence(this.SchemaName, this.Name, startAt);
        this.Parent.Connection.Execute(sql);
        return startAt;
    }
}
