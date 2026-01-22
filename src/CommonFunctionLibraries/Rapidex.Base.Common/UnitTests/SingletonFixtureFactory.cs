using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTests;

public class SingletonFixtureFactory<T> where T : ICoreTestFixture
{
    protected static Dictionary<Type, ICoreTestFixture> fixtures = new Dictionary<Type, ICoreTestFixture>();

    protected static object _lock = new object();

    public T GetFixture()
    {
        lock (_lock)
            try
            {
                if (!fixtures.ContainsKey(typeof(T)))
                {
                    T fixture = TypeHelper.CreateInstance<T>(typeof(T));
                    fixtures.Add(typeof(T), fixture);
                    fixture.Init();
                    fixture.CheckInit();
                }

                return (T)fixtures[typeof(T)];
            }
            catch (Exception ex)
            {
                throw;
            }
    }

}
