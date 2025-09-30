using Rapidex.UnitTest.Base.Common.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Base.Common;

public class DataConvertionTests : IClassFixture<SingletonFixtureFactory<DefaultEmptyFixture>>
{
    DefaultEmptyFixture fixture;

    public DataConvertionTests(SingletonFixtureFactory<DefaultEmptyFixture> factory)
    {
        fixture = factory.GetFixture();
    }

    [Fact]
    public void DateTime_01_Offset()
    {
        var date = new DateTime(2019, 1, 1);
        var offset = new DateTimeOffset(date, TimeSpan.Zero);

        var converter = new RapidexTypeConverter();

        DateTime dt = (DateTime)converter.Convert(offset, typeof(DateTime));
        Assert.Equal(date, dt);

        DateTime dt2 = offset.As<DateTime>();
        Assert.Equal(date, dt2);

        var date2 = default(DateTime);
        DateTimeOffset dto3 = (DateTimeOffset)converter.Convert(date2, typeof(DateTimeOffset));
        Assert.Equal(DateTimeOffset.MinValue, dto3);

        //var date2 = offset.DateTime;
        //Assert.Equal(date, date2);
    }
}
