using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTests;

public class EachTestClassIsolatedFixtureFactory<T> where T : ICoreTestFixture
{
    protected static Dictionary<Type, ICoreTestFixture> fixtures = new Dictionary<Type, ICoreTestFixture>();

    protected static object _lock = new object();

    public T GetFixture(Type classType)
    {
        lock (_lock)
            try
            {
                if (!fixtures.ContainsKey(classType))
                {
                    T fixture = TypeHelper.CreateInstance<T>(typeof(T));
                    fixtures.Add(classType, fixture);
                    fixture.Init();
                    fixture.CheckInit();
                }

                return (T)fixtures[classType];
            }
            catch (Exception)
            {
                throw;
            }
    }

    public T GetFixture<R>()
    {
        return this.GetFixture(typeof(R));
    }
}
