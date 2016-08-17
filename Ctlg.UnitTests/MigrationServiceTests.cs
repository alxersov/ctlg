using Ctlg.Data.Service;
using Ctlg.Db.Migrations;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    [TestFixture]
    public class MigrationServiceTests
    {
        [Test]
        public void LoadMigration_CanLoadMigrationsUpToCurrentVersion()
        {
            var migrationService = new MigrationService();

            for (int v = 1; v <= DataService.RequiredDbVersion; ++v)
            {
                var migration = migrationService.LoadMigration(v);
                Assert.That(migration, Is.Not.Null);
            }
        }
    }
}
