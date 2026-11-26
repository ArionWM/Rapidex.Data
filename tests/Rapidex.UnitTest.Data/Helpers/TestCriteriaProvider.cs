using Rapidex.Data.Query;

namespace Rapidex.UnitTest.Data.Helpers;

/// <summary>
/// Yetkilendirme enjeksiyonunu simüle eden test criteria provider.
/// Ek koşulları <see cref="QueryExtenders.AndOverExisting{T}"/> ile uygular;
/// böylece mevcut where (özellikle OR) grupları bozulmaz.
/// </summary>
internal class TestCriteriaProvider : IDbCriteriaProvider
{
    public int CheckCallCount { get; private set; }
    public Action<IQueryCriteria> Filter { get; set; }

    public TestCriteriaProvider(Action<IQueryCriteria> filter)
    {
        this.Filter = filter.NotNull();
    }

    public void Check<T>(T query) where T : IQueryCriteria
    {
        this.CheckCallCount++;
        query.AndOverExisting(this.Filter);
    }
}
