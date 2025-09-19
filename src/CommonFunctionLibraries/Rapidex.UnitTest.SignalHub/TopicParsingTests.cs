using Rapidex.SignalHub;
using Rapidex.UnitTest.Base.Common.Fixtures;

namespace Rapidex.UnitTest.SignalHub;

public class TopicParsing : IClassFixture<SingletonFixtureFactory<DefaultEmptyFixture>>
{
    DefaultEmptyFixture fixture;

    public TopicParsing(SingletonFixtureFactory<DefaultEmptyFixture> factory)
    {
        fixture = factory.GetFixture();
    }

    [Fact]
    public void Basics_01()
    {
        var result = TopicParser.Parse(null, "myInvalid1");
        Assert.False(result.Valid);
        Assert.Null(result.Topic);

        result = TopicParser.Parse(null, "tenant1/workspace1/hasNoSignal/");
        Assert.False(result.Valid);
        Assert.Null(result.Topic);

        result = TopicParser.Parse(null, "tenant1/workspace1/module1/unmatchedSignal/entity1");
        Assert.True(result.Valid);
        Assert.False(result.Match);
        Assert.NotNull(result.Topic);
        Assert.False(result.Topic.IsSystemLevel);

        //Hub olmadýðý için EntityReleated olduðunu anlayamaz.
        Assert.Null(result.Topic.Entity);
        Assert.Null(result.Topic.EntityId);
        Assert.NotEmpty(result.Topic.OtherSections);


        result = TopicParser.Parse(null, "+/workspace1/module1/unmatchedSignal/entity1");
        Assert.True(result.Valid);
        Assert.False(result.Match);
        Assert.NotNull(result.Topic);
        Assert.True(result.Topic.IsSystemLevel);

        result = TopicParser.Parse(null, "tenant1/workspace1/module1/+/entity1");
        Assert.False(result.Valid);
    }
}