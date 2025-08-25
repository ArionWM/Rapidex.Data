using Rapidex.Data;
using Rapidex.Data.SqlServer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.Fixtures
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbWithProviderFixture<T> : DbFixture, ICoreTestFixture where T : IDbProvider
    {


        public DbWithProviderFixture()
        {
            this.Init();
        }

        public override void Init()
        {
            //Database.Configuration.DatabaseSectionParentName = typeof(T).Name;
            base.Init();

            Database.Scopes.AddMainDbIfNotExists();
        }



    }
}
