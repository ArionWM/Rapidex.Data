namespace Rapidex.UnitTest.Data.SqlServer;

public class SqlServer_02_TableStructureTests : TableStructureTestsBase<DbSqlServerProvider>
{
    public SqlServer_02_TableStructureTests(SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }

    protected override string[] GetTableNamesInSchema(IDbSchemaScope schemaScope)
    {
        using DbSqlServerConnection connection = Rapidex.Data.SqlServer.Library.CreateSqlServerConnection();

        DataTable table = connection.Execute($"SELECT t.name FROM sys.tables AS t INNER JOIN sys.schemas AS s ON t.[schema_id] = s.[schema_id] WHERE s.name = N'{schemaScope.SchemaName}';");

        List<string> names = new List<string>();

        foreach (DataRow row in table.Rows)
        {
            names.Add(row[0].ToString());
        }

        return names.ToArray();
    }

    public override void Structure_01_CreateTable_FromJson()
    {
        base.Structure_01_CreateTable_FromJson();


        using DbSqlServerConnection connection = Rapidex.Data.SqlServer.Library.CreateSqlServerConnection();

        connection.Execute($"USE [{Database.Dbs.Db().DatabaseName}]");

        DataTable table = connection.Execute($"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'base' AND TABLE_NAME = 'utest_myJsonEntity03'");
        Assert.Equal(1, table.Rows.Count);

        DataTable columnsTable = connection.Execute($"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'base' AND TABLE_NAME = 'utest_myJsonEntity03'");

        DataView dv = new DataView(columnsTable);

        dv.RowFilter = "COLUMN_NAME = 'Id'";

        Assert.Single(dv);
        Assert.Equal("NO", dv[0]["IS_NULLABLE"]);
        Assert.Equal("bigint", dv[0]["DATA_TYPE"]);
        Assert.Equal(DBNull.Value, dv[0]["CHARACTER_MAXIMUM_LENGTH"]);

        dv.RowFilter = "COLUMN_NAME = 'Subject'";
        Assert.Single(dv);
        Assert.Equal("YES", dv[0]["IS_NULLABLE"]);
        Assert.Equal("nvarchar", dv[0]["DATA_TYPE"]);
        Assert.Equal(250, dv[0]["CHARACTER_MAXIMUM_LENGTH"]);

        dv.RowFilter = "COLUMN_NAME = 'StartTime'";
        Assert.Single(dv);
        Assert.Equal("YES", dv[0]["IS_NULLABLE"]);
        Assert.Equal("datetimeoffset", dv[0]["DATA_TYPE"]);
        Assert.Equal(DBNull.Value, dv[0]["CHARACTER_MAXIMUM_LENGTH"]);

        dv.RowFilter = "COLUMN_NAME = 'ConcreteReference01'";
        Assert.Single(dv);
        Assert.Equal("YES", dv[0]["IS_NULLABLE"]);
        Assert.Equal("bigint", dv[0]["DATA_TYPE"]);
        Assert.Equal(DBNull.Value, dv[0]["CHARACTER_MAXIMUM_LENGTH"]);
    }

    public override void Structure_02_CreateTable_FromConcrete()
    {
        base.Structure_02_CreateTable_FromConcrete();

        using DbSqlServerConnection connection = Rapidex.Data.SqlServer.Library.CreateSqlServerConnection();

        DataTable table = connection.Execute($"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'base' AND TABLE_NAME = 'utest_ConcreteEntity01'");
        Assert.Equal(1, table.Rows.Count);

        DataTable columnsTable = connection.Execute($"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'base' AND TABLE_NAME = 'utest_ConcreteEntity01'");

        DataView dv = new DataView(columnsTable);

        dv.RowFilter = "COLUMN_NAME = 'Id'";

        Assert.Single(dv);
        Assert.Equal("NO", dv[0]["IS_NULLABLE"]);
        Assert.Equal("bigint", dv[0]["DATA_TYPE"]);
        Assert.Equal(DBNull.Value, dv[0]["CHARACTER_MAXIMUM_LENGTH"]);

        dv.RowFilter = "COLUMN_NAME = 'Name'";
        Assert.Single(dv);
        Assert.Equal("YES", dv[0]["IS_NULLABLE"]);
        Assert.Equal("nvarchar", dv[0]["DATA_TYPE"]);
        Assert.Equal(250, dv[0]["CHARACTER_MAXIMUM_LENGTH"]);

    }

    public override void Structure_03_AddColumnInRuntime()
    {
        base.Structure_03_AddColumnInRuntime();
        var dbScope = Database.Dbs.Db();

        using DbSqlServerConnection connection = Rapidex.Data.SqlServer.Library.CreateSqlServerConnection();

        DataTable columnsTable = connection.Execute($"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'base' AND TABLE_NAME = 'utest_myJsonEntity04'");

        DataView dv = new DataView(columnsTable);

        dv.RowFilter = "COLUMN_NAME = 'Id'";

        Assert.Single(dv);
        Assert.Equal("NO", dv[0]["IS_NULLABLE"]);
        Assert.Equal("bigint", dv[0]["DATA_TYPE"]);
        Assert.Equal(DBNull.Value, dv[0]["CHARACTER_MAXIMUM_LENGTH"]);


        dv.RowFilter = "COLUMN_NAME = 'AddedField'";
        Assert.Empty(dv);

        var em = dbScope.Metadata.Get("myJsonEntity04");
        em.AddFieldIfNotExist<int>("AddedField");
        Database.Dbs.Db().Base.Structure.ApplyEntityStructure(em);

        columnsTable = connection.Execute($"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'base' AND TABLE_NAME = 'utest_myJsonEntity04'");
        dv = new DataView(columnsTable);

        dv.RowFilter = "COLUMN_NAME = 'AddedField'";
        Assert.Single(dv);
        Assert.Equal("YES", dv[0]["IS_NULLABLE"]);
        Assert.Equal("int", dv[0]["DATA_TYPE"]);

    }

    public override void Structure_05_RuntimeModify()
    {
        base.Structure_05_RuntimeModify();

        var dbScope = Database.Dbs.Db();

        using DbSqlServerConnection connection = Rapidex.Data.SqlServer.Library.CreateSqlServerConnection();

        DataTable columnsTable = connection.Execute($"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'base' AND TABLE_NAME = 'utest_myJsonEntity04'");

        DataView dv = new DataView(columnsTable);

        dv.RowFilter = "COLUMN_NAME = 'Id'";

        Assert.Single(dv);
        Assert.Equal("NO", dv[0]["IS_NULLABLE"]);
        Assert.Equal("bigint", dv[0]["DATA_TYPE"]);
        Assert.Equal(DBNull.Value, dv[0]["CHARACTER_MAXIMUM_LENGTH"]);


        dv.RowFilter = "COLUMN_NAME = 'AddedField'";
        Assert.Empty(dv);

        var em = dbScope.Metadata.Get("myJsonEntity04");
        var fm = em.Fields["ChangeField"];

        columnsTable = connection.Execute($"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'base' AND TABLE_NAME = 'utest_myJsonEntity04'");
        dv = new DataView(columnsTable);
        dv.RowFilter = "COLUMN_NAME = 'ChangeField'";
        Assert.Single(dv);
        Assert.Equal("YES", dv[0]["IS_NULLABLE"]);
        Assert.Equal("int", dv[0]["DATA_TYPE"]);
        Assert.Equal(DBNull.Value, dv[0]["CHARACTER_MAXIMUM_LENGTH"]);

        //Değiştirelim
        fm.Type = typeof(string);
        fm.BaseType = typeof(string);
        fm.DbType = DbFieldType.String;
        fm.DbProperties.Length = 250;

        Database.Dbs.Db().Base.Structure.ApplyEntityStructure(em);

        columnsTable = connection.Execute($"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'base' AND TABLE_NAME = 'utest_myJsonEntity04'");
        dv = new DataView(columnsTable);

        dv.RowFilter = "COLUMN_NAME = 'ChangeField'";
        Assert.Single(dv);
        Assert.Equal("YES", dv[0]["IS_NULLABLE"]);
        Assert.Equal("nvarchar", dv[0]["DATA_TYPE"]);
        Assert.Equal(250, dv[0]["CHARACTER_MAXIMUM_LENGTH"]);



    }
}
