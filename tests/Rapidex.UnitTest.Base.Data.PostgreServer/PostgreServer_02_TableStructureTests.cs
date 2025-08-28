using Rapidex.Data.PostgreServer;

namespace Rapidex.UnitTest.Data.PostgreServer
{
    public class PostgreServer_02_TableStructureTests : TableStructureTestsBase<PostgreSqlServerProvider>
    {
        public PostgreServer_02_TableStructureTests(SingletonFixtureFactory<DbWithProviderFixture<PostgreSqlServerProvider>> factory) : base(factory)
        {
        }

        protected override string[] GetTableNamesInSchema(IDbSchemaScope schemaScope)
        {
            using PostgreSqlServerConnection connection = Library.CreatePostgreServerConnection();

            string sql = $"SELECT table_name FROM information_schema.tables WHERE table_schema = '{schemaScope.SchemaName.ToLowerInvariant()}' AND table_type = 'BASE TABLE';";
            DataTable table = connection.Execute(sql);

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

            using PostgreSqlServerConnection connection = Library.CreatePostgreServerConnection();

            // PostgreSQL doesn't support USE statement, connection is already to the correct database
            // connection.Execute($"USE [{db.DatabaseName}]");

            string sql = $"SELECT table_name FROM information_schema.tables WHERE table_schema = 'base' AND table_name = 'utest_myjsonentity03';";
            DataTable table = connection.Execute(sql);
            Assert.Equal(1, table.Rows.Count);

            DataTable columnsTable = connection.Execute($"SELECT * FROM information_schema.columns WHERE table_schema = 'base' AND table_name = 'utest_myjsonentity03'");

            DataView dv = new DataView(columnsTable);

            dv.RowFilter = "column_name = 'id'";

            Assert.Single(dv);
            Assert.Equal("NO", dv[0]["is_nullable"]);
            Assert.Equal("bigint", dv[0]["data_type"]);
            Assert.Equal(DBNull.Value, dv[0]["character_maximum_length"]);

            dv.RowFilter = "column_name = 'subject'";
            Assert.Single(dv);
            Assert.Equal("YES", dv[0]["is_nullable"]);
            Assert.Equal("character varying", dv[0]["data_type"]);
            Assert.Equal(250, dv[0]["character_maximum_length"]);

            dv.RowFilter = "column_name = 'starttime'";
            Assert.Single(dv);
            Assert.Equal("YES", dv[0]["is_nullable"]);
            Assert.Equal("timestamp with time zone", dv[0]["data_type"]);
            Assert.Equal(DBNull.Value, dv[0]["character_maximum_length"]);

            dv.RowFilter = "column_name = 'concretereference01'";
            Assert.Single(dv);
            Assert.Equal("YES", dv[0]["is_nullable"]);
            Assert.Equal("bigint", dv[0]["data_type"]);
            Assert.Equal(DBNull.Value, dv[0]["character_maximum_length"]);
        }

        public override void Structure_02_CreateTable_FromConcrete()
        {
            base.Structure_02_CreateTable_FromConcrete();

            using PostgreSqlServerConnection connection = Library.CreatePostgreServerConnection();

            DataTable table = connection.Execute($"SELECT * FROM information_schema.tables WHERE table_schema = 'base' AND table_name = 'utest_concreteentity01'");
            Assert.Equal(1, table.Rows.Count);

            DataTable columnsTable = connection.Execute($"SELECT * FROM information_schema.columns WHERE table_schema = 'base' AND table_name = 'utest_concreteentity01'");

            DataView dv = new DataView(columnsTable);

            dv.RowFilter = "column_name = 'id'";

            Assert.Single(dv);
            Assert.Equal("NO", dv[0]["is_nullable"]);
            Assert.Equal("bigint", dv[0]["data_type"]);
            Assert.Equal(DBNull.Value, dv[0]["character_maximum_length"]);

            dv.RowFilter = "column_name = 'name'";
            Assert.Single(dv);
            Assert.Equal("YES", dv[0]["is_nullable"]);
            Assert.Equal("character varying", dv[0]["data_type"]);
            Assert.Equal(250, dv[0]["character_maximum_length"]);

        }

        public override void Structure_03_AddColumnInRuntime()
        {
            base.Structure_03_AddColumnInRuntime();

            var db = Database.Dbs.Db();

            using PostgreSqlServerConnection connection = Library.CreatePostgreServerConnection();

            DataTable columnsTable = connection.Execute($"SELECT * FROM information_schema.columns WHERE table_schema = 'base' AND table_name = 'utest_myjsonentity04'");

            DataView dv = new DataView(columnsTable);

            dv.RowFilter = "column_name = 'id'";

            Assert.Single(dv);
            Assert.Equal("NO", dv[0]["is_nullable"]);
            Assert.Equal("bigint", dv[0]["data_type"]);
            Assert.Equal(DBNull.Value, dv[0]["character_maximum_length"]);


            dv.RowFilter = "column_name = 'addedfield'";
            Assert.Empty(dv);

            var em = db.Metadata.Get("myJsonEntity04");
            em.AddFieldIfNotExist<int>("AddedField");
            db.Base.Structure.ApplyEntityStructure(em);

            columnsTable = connection.Execute($"SELECT * FROM information_schema.columns WHERE table_schema = 'base' AND table_name = 'utest_myjsonentity04'");
            dv = new DataView(columnsTable);

            dv.RowFilter = "column_name = 'addedfield'";
            Assert.Single(dv);
            Assert.Equal("YES", dv[0]["is_nullable"]);
            Assert.Equal("integer", dv[0]["data_type"]);

        }

        public override void Structure_05_RuntimeModify()
        {
            base.Structure_05_RuntimeModify();

            var db = Database.Dbs.Db();

            using PostgreSqlServerConnection connection = Library.CreatePostgreServerConnection();

            DataTable columnsTable = connection.Execute($"SELECT * FROM information_schema.columns WHERE table_schema = 'base' AND table_name = 'utest_myjsonentity04'");

            DataView dv = new DataView(columnsTable);

            dv.RowFilter = "column_name = 'id'";

            Assert.Single(dv);
            Assert.Equal("NO", dv[0]["is_nullable"]);
            Assert.Equal("bigint", dv[0]["data_type"]);
            Assert.Equal(DBNull.Value, dv[0]["character_maximum_length"]);


            dv.RowFilter = "column_name = 'addedfield'";
            Assert.Empty(dv);

            var em = db.Metadata.Get("myJsonEntity04");
            var fm = em.Fields["ChangeField"];

            columnsTable = connection.Execute($"SELECT * FROM information_schema.columns WHERE table_schema = 'base' AND table_name = 'utest_myjsonentity04'");
            dv = new DataView(columnsTable);
            dv.RowFilter = "column_name = 'changefield'";
            Assert.Single(dv);
            Assert.Equal("YES", dv[0]["is_nullable"]);
            Assert.Equal("integer", dv[0]["data_type"]);
            Assert.Equal(DBNull.Value, dv[0]["character_maximum_length"]);

            //Değiştirelim
            fm.Type = typeof(string);
            fm.BaseType = typeof(string);
            fm.DbType = DbFieldType.String;
            fm.DbProperties.Length = 250;

            db.Base.Structure.ApplyEntityStructure(em);

            columnsTable = connection.Execute($"SELECT * FROM information_schema.columns WHERE table_schema = 'base' AND table_name = 'utest_myjsonentity04'");
            dv = new DataView(columnsTable);

            dv.RowFilter = "column_name = 'changefield'";
            Assert.Single(dv);
            Assert.Equal("YES", dv[0]["is_nullable"]);
            Assert.Equal("character varying", dv[0]["data_type"]);
            Assert.Equal(250, dv[0]["character_maximum_length"]);

        }
    }
}
