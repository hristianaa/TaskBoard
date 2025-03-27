using TaskBoard.Data;
using TaskBoard.Tests.Common;

using NUnit.Framework;

namespace TaskBoard.WebApp.Tests
{
    public class UnitTestsBase
    {
        protected TestDb testDb;
        protected ApplicationDbContext dbContext;

        [OneTimeSetUp]
        public void OneTimeSetupBase()
        {
            this.testDb = new TestDb();
            this.dbContext = this.testDb.CreateDbContext();
        }
    }
}
