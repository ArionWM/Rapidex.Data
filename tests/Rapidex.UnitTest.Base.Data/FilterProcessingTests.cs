using Microsoft.Extensions.DependencyInjection;
using Rapidex.Data.Query;
using Rapidex.UnitTest.Data.TestContent;
using SqlKata.Compilers;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rapidex.Data.Parsing;
using Superpower;
using Superpower.Display;
using Superpower.Parsers;
using Superpower.Tokenizers;
using System.Net.Sockets;



namespace Rapidex.UnitTest.Data;

public class FilterProcessingTests : DbDependedTestsBase<DbSqlServerProvider>
{
    internal FilterTextParser CriteriaParser { get; }

    public FilterProcessingTests(SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
        this.CriteriaParser = new FilterTextParser();
    }

    //[Fact]
    //public void XXX()
    //{
    //    string input = "(a > 1 | (b = 2))"; //(c | d)
    //    var tokens = TestParser.tokenizer.Tokenize(input);

    //    var result = TestParser.Expression2.TryParse(tokens);
    //    if (result.HasValue)
    //    {
    //        // input is valid
    //        var expression = (Expression)result.Value;
    //        Console.WriteLine(expression);
    //        // do what you need with it here, i.e. loop through the nodes, output the text, etc.
    //    }
    //    else
    //    {
    //        Console.WriteLine("NO VALUE");
    //        // not valid
    //    }
    //}


    [Fact]
    public void FilterParser_01()
    {
        //[FromKeyedServices(Rapidex.Common.ENV)] SimpleFlatCriteriaParser criteriaParser
        //this.Fixture.ClearCaches();

        var db = Database.Scopes.Db();

        db.Metadata.AddIfNotExist<ConcreteEntityForFilterTest>();
        db.Structure.ApplyEntityStructure<ConcreteEntityForFilterTest>();

        var query = db.GetQuery<ConcreteEntityForFilterTest>();

        //------------------------------------------------------------------------------
        string filter = "Name=abc12";
        var criteria = CriteriaParser.Parse(query, filter);

        IEnumerable<SqlKata.BasicCondition> conditions = criteria.Query.Clauses.Where(cl => cl is SqlKata.BasicCondition).Cast<SqlKata.BasicCondition>();
        Assert.NotEmpty(conditions);
        Assert.Single(conditions);

        SqlKata.BasicCondition cond01 = conditions.First();
        Assert.Contains(".Name", cond01.Column, StringComparison.InvariantCultureIgnoreCase);
        Assert.Equal("=", cond01.Operator);
        Assert.Equal("abc12", cond01.Value);

        //------------------------------------------------------------------------------
        query = db.GetQuery<ConcreteEntityForFilterTest>();
        filter = "Address~addr1*";
        criteria = CriteriaParser.Parse(query, filter);

        conditions = criteria.Query.Clauses.Where(cl => cl is SqlKata.BasicCondition).Cast<SqlKata.BasicCondition>();
        Assert.NotEmpty(conditions);
        Assert.Single(conditions);

        SqlKata.BasicCondition cond02 = conditions.First();

        Assert.Contains(".Address", cond02.Column, StringComparison.InvariantCultureIgnoreCase);
        Assert.Equal("like", cond02.Operator);
        Assert.Equal("addr1%", cond02.Value);


        //------------------------------------------------------------------------------
        query = db.GetQuery<ConcreteEntityForFilterTest>();
        filter = "Age > 18 & Name like myName*";
        criteria = CriteriaParser.Parse(query, filter);

        var nestedCondition = criteria.Query.Clauses.Where(cl => cl is SqlKata.NestedCondition<SqlKata.Query>).Cast<SqlKata.NestedCondition<SqlKata.Query>>();
        Assert.NotEmpty(nestedCondition);
        Assert.Single(nestedCondition);

        conditions = nestedCondition.FirstOrDefault().Query.Clauses.Where(cl => cl is SqlKata.BasicCondition).Cast<SqlKata.BasicCondition>();
        Assert.NotEmpty(conditions);
        Assert.Equal(2, conditions.Count());

        SqlKata.BasicCondition cond02_1 = conditions.ElementAt(0);
        SqlKata.BasicCondition cond02_2 = conditions.ElementAt(1);

        Assert.Contains(".Age", cond02_1.Column, StringComparison.InvariantCultureIgnoreCase);
        Assert.Equal(">", cond02_1.Operator);
        Assert.Equal("18", cond02_1.Value);

        Assert.Contains(".Name", cond02_2.Column, StringComparison.InvariantCultureIgnoreCase);
        Assert.Equal("like", cond02_2.Operator);
        Assert.Equal("myName%", cond02_2.Value);




        //------------------------------------------------------------------------------
        query = db.GetQuery<ConcreteEntityForFilterTest>();
        filter = "(Name=abc12) & ((Date>today) | (date=null))";
        criteria = CriteriaParser.Parse(query, filter);

        nestedCondition = criteria.Query.Clauses.Where(cl => cl is SqlKata.NestedCondition<SqlKata.Query>).Cast<SqlKata.NestedCondition<SqlKata.Query>>();
        Assert.NotEmpty(nestedCondition);
        Assert.Single(nestedCondition);

        //Two conditions 1: basic, 2: nested
        conditions = nestedCondition.FirstOrDefault().Query.Clauses.Where(cl => cl is SqlKata.BasicCondition).Cast<SqlKata.BasicCondition>();
        Assert.NotEmpty(conditions);
        Assert.Single(conditions);

        var nestedCondition2 = nestedCondition.First().Query.Clauses.Where(cl => cl is SqlKata.NestedCondition<SqlKata.Query>).Cast<SqlKata.NestedCondition<SqlKata.Query>>();
        Assert.NotEmpty(nestedCondition);
        Assert.Single(nestedCondition);

        var conditions2 = nestedCondition2.First().Query.Clauses.Where(cl => cl is SqlKata.NestedCondition<SqlKata.Query>).Cast<SqlKata.NestedCondition<SqlKata.Query>>();
        Assert.NotEmpty(conditions2);
        Assert.Equal(2, conditions2.Count());


        SqlKata.BasicCondition cond03_1 = conditions.ElementAt(0);

        SqlKata.BasicCondition cond03_2_1 = conditions2.First().Query.Clauses.Where(cl => cl is SqlKata.BasicCondition).Cast<SqlKata.BasicCondition>().FirstOrDefault();
        SqlKata.NullCondition cond03_2_2 = conditions2.Last().Query.Clauses.Where(cl => cl is SqlKata.NullCondition).Cast<SqlKata.NullCondition>().FirstOrDefault();
        Assert.NotNull(cond03_2_1);
        Assert.NotNull(cond03_2_2);

        Assert.Contains(".Name", cond03_1.Column, StringComparison.InvariantCultureIgnoreCase);
        Assert.Equal("=", cond03_1.Operator);
        Assert.Equal("abc12", cond03_1.Value);

        Assert.Contains(".Date", cond03_2_1.Column, StringComparison.InvariantCultureIgnoreCase);
        Assert.Equal(">", cond03_2_1.Operator);
        Assert.IsType<string>(cond03_2_1.Value);
        Assert.True(DateTimeOffset.TryParse((string)cond03_2_1.Value, out DateTimeOffset _));

        Assert.Contains(".date", cond03_2_2.Column, StringComparison.InvariantCultureIgnoreCase);
        //Assert.Equal("=", cond03_2_2.Operator);
        //Assert.Equal("null", cond03_2_2.Value);


        //------------------------------------------------------------------------------
        //Enumerations
        query = db.GetQuery<ConcreteEntityForFilterTest>();
        filter = "ContactType=Department";
        criteria = CriteriaParser.Parse(query, filter);

        conditions = criteria.Query.Clauses.Where(cl => cl is SqlKata.BasicCondition).Cast<SqlKata.BasicCondition>();
        Assert.NotEmpty(conditions);
        Assert.Single(conditions);

        SqlKata.BasicCondition cond04 = conditions.First();
        Assert.Contains(".ContactType", cond04.Column, StringComparison.InvariantCultureIgnoreCase);
        Assert.Equal("=", cond04.Operator);
        Assert.Equal(2L, cond04.Value);

        //------------------------------------------------------------------------------
        //Null
        query = db.GetQuery<ConcreteEntityForFilterTest>();
        filter = "Name=null";
        criteria = CriteriaParser.Parse(query, filter);

        var nullConditions = criteria.Query.Clauses.Where(cl => cl is SqlKata.NullCondition).Cast<SqlKata.NullCondition>();
        Assert.NotEmpty(nullConditions);
        Assert.Single(nullConditions);

        SqlKata.NullCondition cond05 = nullConditions.First();
        Assert.Contains(".Name", cond05.Column, StringComparison.InvariantCultureIgnoreCase);
        //Assert.Equal("=", cond05.Operator);
        //Assert.Equal(2L, cond05.Value);

        //------------------------------------------------------------------------------
        //Caption -> Name
        query = db.GetQuery<ConcreteEntityForFilterTest>();
        filter = "Caption=abc";
        criteria = CriteriaParser.Parse(query, filter);

        conditions = criteria.Query.Clauses.Where(cl => cl is SqlKata.BasicCondition).Cast<SqlKata.BasicCondition>();
        Assert.NotEmpty(conditions);
        Assert.Single(conditions);

        SqlKata.BasicCondition cond06 = conditions.First();
        Assert.Contains(".Name", cond06.Column, StringComparison.InvariantCultureIgnoreCase);
        //Assert.Equal("=", cond06.Operator);
        //Assert.Equal(2L, cond06.Value);



        ////------------------------------------------------------------------------------
        //query = database.GetQuery<ConcreteEntityForFilterTest>();
        //filter = "(Name=1) and (Address~addr1* or Address~addr2*)";
        //criteria = CriteriaParser.Parse(query, filter);

        //conditions = criteria.Query.Clauses.Where(cl => cl is SqlKata.BasicCondition).Cast<SqlKata.BasicCondition>();
        //Assert.NotEmpty(conditions);
        //Assert.Single(conditions);


        ////------------------------------------------------------------------------------
        //query = database.GetQuery<ConcreteEntityForFilterTest>();
        //filter = "Name=null";
        //criteria = CriteriaParser.Parse(query, filter);

        //conditions = criteria.Query.Clauses.Where(cl => cl is SqlKata.BasicCondition).Cast<SqlKata.BasicCondition>();
        //Assert.NotEmpty(conditions);
        //Assert.Single(conditions);





        //SqlServerCompiler mySqlCompiler = new SqlServerCompiler();
        //SqlResult result = mySqlCompiler.Compile(criteria.Query);
        //string sql = result.ToString();

        //Assert.NotNull(sql);
        ////SELECT * FROM [Base].[ConcreteEntityForFilterTest] AS [C36516] WHERE [C36516].[Name] = 'abc12' AND LOWER([C36516].[Address]) like 'addr1%'

        //Assert.Contains("[Name] = abc12", sql);
        //Assert.Contains("[Address]) like addr1*", sql);

    }

    [Fact]
    public async Task FilterParser_02()
    {
        //this.Fixture.ClearCaches();

        var db = Database.Scopes.Db();
        db.Metadata.AddIfNotExist<ConcreteEntityForN2NTest01>();
        db.Metadata.AddIfNotExist<ConcreteEntityForN2NTest02>();

        db.Structure.DropEntity<ConcreteEntityForN2NTest01>();
        db.Structure.DropEntity<ConcreteEntityForN2NTest02>();
        db.Structure.ApplyEntityStructure<ConcreteEntityForN2NTest01>();
        db.Structure.ApplyEntityStructure<ConcreteEntityForN2NTest02>();

        ConcreteEntityForN2NTest01 master01 = db.New<ConcreteEntityForN2NTest01>();
        master01.Name = "master01";
        master01.Save();

        ConcreteEntityForN2NTest02 detail01_01 = db.New<ConcreteEntityForN2NTest02>();
        detail01_01.Name = "detail01-01";
        detail01_01.Save();
        master01.Relation01.Add(detail01_01);

        //ConcreteEntityForN2NTest02 detail01_02 = database.New<ConcreteEntityForN2NTest02>();
        //detail01_02.Name = "detail01-02";
        //detail01_02.Save();
        //master01.Relation01.Add(detail01_02);


        ConcreteEntityForN2NTest01 master02 = db.New<ConcreteEntityForN2NTest01>();
        master02.Name = "master02";
        master02.Save();

        ConcreteEntityForN2NTest02 detail02_01 = db.New<ConcreteEntityForN2NTest02>();
        detail02_01.Name = "detail02-02";
        detail02_01.Save();
        master02.Relation01.Add(detail02_01);

        ConcreteEntityForN2NTest02 detail02_02 = db.New<ConcreteEntityForN2NTest02>();
        detail02_02.Name = "detail02-02";
        detail02_02.Save();
        master02.Relation01.Add(detail02_02);

        await db.ApplyChanges();


        long masterId01 = master01.Id;
        long masterId02 = master02.Id;

        /*
          
         
         EntityB yı getir, ancak EntityA'nin MyField field'ında olanları
         
         Releated = EntityA/123/MyField
         Releated = EntityA/123/MyField
         
         */

        var query01 = db.GetQuery<ConcreteEntityForN2NTest02>();
        string filter01 = $"related = ConcreteEntityForN2NTest01/{masterId01}/Relation01";
        this.CriteriaParser.Parse(query01, filter01);
        IEntityLoadResult<ConcreteEntityForN2NTest02> result01 = await query01.Load();
        Assert.Equal(1, result01.Count());


        var query02 = db.GetQuery<ConcreteEntityForN2NTest02>();
        string filter02 = $"related = ConcreteEntityForN2NTest01/{masterId02}/Relation01";
        this.CriteriaParser.Parse(query02, filter02);
        IEntityLoadResult<ConcreteEntityForN2NTest02> result02 = await query02.Load();
        Assert.Equal(2, result02.Count());

    }

}
