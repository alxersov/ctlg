using System.Collections.Generic;
using Ctlg.Data;
using Ctlg.Db.Migrations;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    [TestFixture]
    public class DataServiceTests
    {
        [Test]
        public void ApplyDbMigrations_WhenDbAt0Version_AppliesAllMigrationsInCorrectOrder()
        {
            var mirgationServiceFake = new Mock<IMigrationService>();
            mirgationServiceFake.Setup(m => m.LoadMigration(It.IsAny<int>()))
                .Returns((int dbVersion) => $"Migration {dbVersion}");

            var ctlgContextFake = new Mock<ICtlgContext>();
            ctlgContextFake.Setup(d => d.DbVersion).Returns(0);

            var appliedMigrations = new List<string>();
            ctlgContextFake.Setup(d => d.ApplyMigration(It.IsAny<string>(), It.IsAny<int>()))
                .Callback<string, int>((migration, dbVersion) => appliedMigrations.Add(migration));

            var dataService = new DataService(mirgationServiceFake.Object, ctlgContextFake.Object);
            dataService.ApplyDbMigrations();

            mirgationServiceFake.Verify(m => m.LoadMigration(It.IsAny<int>()),
                Times.Exactly(DataService.RequiredDbVersion));
            ctlgContextFake.Verify(d => d.ApplyMigration(It.IsAny<string>(), It.IsAny<int>()),
                Times.Exactly(DataService.RequiredDbVersion));

            for (var i = 0; i < DataService.RequiredDbVersion; ++i)
            {
                Assert.That(appliedMigrations[i], Is.EqualTo($"Migration {i + 1}"));
            }
        }
    }
}
