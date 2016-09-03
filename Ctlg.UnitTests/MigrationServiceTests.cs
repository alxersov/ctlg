using System;
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

        [Test]
        public void LoadMigration_LoadingMigrationThatDoesNotExists_ThrowsException()
        {
            var migrationService = new MigrationService();
            var badMigrationVersion = DataService.RequiredDbVersion + 1;
            Assert.That(() => { migrationService.LoadMigration(badMigrationVersion); },
                Throws.InstanceOf<Exception>().With
                .Message.Contain("not found"));
        }
    }
}
