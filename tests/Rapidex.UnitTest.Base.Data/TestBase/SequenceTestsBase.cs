using Rapidex.Data;

using Rapidex.UnitTest.Data.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestBase
{
    public abstract class SequenceTestsBase<T> : DbDependedTestsBase<T> where T : IDbProvider
    {
        protected SequenceTestsBase(SingletonFixtureFactory<DbWithProviderFixture<T>> factory) : base(factory)
        {

        }

        [Fact]
        public void Sequence_01_CreateSequence()
        {
            var dbScope = Database.Scopes.Db();

            string sequenceName = "s" + RandomHelper.RandomText(10);
            dbScope.Structure.CreateSequenceIfNotExists(sequenceName, 1);
            var sequence = dbScope.Data.Sequence(sequenceName);

            Assert.True(dbScope.Data.Sequence(sequenceName).CurrentValue == 1);

            sequenceName = "s" + RandomHelper.RandomText(10);
            dbScope.Structure.CreateSequenceIfNotExists(sequenceName, 10000);
            sequence = dbScope.Data.Sequence(sequenceName);

            Assert.True(dbScope.Data.Sequence(sequenceName).CurrentValue == 10000);
        }

        [Fact]
        public void Sequence_02_GetNext()
        {
            var dbScope = Database.Scopes.Db();

            string sequenceName = "s" + RandomHelper.RandomText(10);
            dbScope.Structure.CreateSequenceIfNotExists(sequenceName, 1);
            var sequence = dbScope.Data.Sequence(sequenceName);

            Assert.True(sequence.GetNext() == 1);
            Assert.True(sequence.GetNext() == 2);
        }

        [Fact]
        public void Sequence_03_Relocate()
        {
            var dbScope = Database.Scopes.Db();

            string sequenceName = "s" + RandomHelper.RandomText(10);
            dbScope.Structure.CreateSequenceIfNotExists(sequenceName, 10001);
            var sequence = dbScope.Data.Sequence(sequenceName);

            sequence.Relocate(20001);

            Assert.True(sequence.GetNext() == 20001);
            Assert.True(sequence.GetNext() == 20002);
        }

        //[Fact]
        //public void Sequence_03_CantRelocateUnderMinValue()
        //{
        //    var dbScope = Database.Scopes.Db();

        //    string sequenceName = RandomHelper.RandomText(10);
        //    dbScope.Structure.AddOrUpdateSequence(sequenceName);
        //    var sequence = dbScope.Data.Sequence(sequenceName);

        //    sequence.Relocate(20001);

        //    Assert.True(sequence.GetNext() == 20001);
        //    Assert.True(sequence.GetNext() == 20002);
        //}


    }
}
