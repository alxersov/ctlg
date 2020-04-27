using System;
using Ctlg.Service.Commands;
using Ctlg.UnitTests.Fixtures;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Commands
{
    public class RestoreCommandTests: CommonDependenciesFixture
    {
        private string HelloHash = "185f8db32271fe25f561a6fc938b2e264306ec304eda518007d1764826381969";

        private string ResotrePath = "destination/a";
        private string BackupName = "Backup1";
        private string Date = "2020-04-25";

        [Test]
        public void Execute_WhenSnapshotNotFound_ThrowsException()
        {
            Assert.That(() => Execute(),
                Throws.TypeOf<Exception>()
                    .With.Message.Contain($"Snapshot {BackupName} is not found"));
        }

        [Test]
        public void Execute_CopiesFileToCorrectDestination()
        {
            CreateSnapshot();

            Execute();

            Assert.That(FS.GetFileAsString($"destination/a/foo/hi.txt"), Is.EqualTo("Hello"));
        }

        private void Execute()
        {
            var command = AutoMock.Create<RestoreCommand>();

            command.Path = ResotrePath;
            command.Name = BackupName;
            command.Date = Date;

            command.Execute(Factories.Config);
        }

        private void CreateSnapshot()
        {
            FS.SetFile($"home/file_storage/18/{HelloHash}", "Hello");
            FS.SetFile($"home/snapshots/{BackupName}/2020-04-25_00-00-00", $"{HelloHash} 2020-01-02T00:00:00.0000000 5 foo/hi.txt\n");
        }
    }
}
