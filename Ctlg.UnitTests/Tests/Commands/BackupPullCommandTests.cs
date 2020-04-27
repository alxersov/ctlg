using System;
using Ctlg.Service.Commands;
using NUnit.Framework;
using Ctlg.UnitTests.Fixtures;
using System.Linq;

namespace Ctlg.UnitTests.Tests.Commands
{
    public class BackupPullCommandTests: CommonDependenciesFixture
    {
        private string HelloHash = "185f8db32271fe25f561a6fc938b2e264306ec304eda518007d1764826381969";

        private string Path = "another/backup";
        private string BackupName = "MyBackup";
        private string Date = "2020-04";

        [Test]
        public void WhenSourceSnapshotNotFound()
        {
            Assert.That(() => Execute(),
                Throws.TypeOf<Exception>().With.Message.Contain($"Snapshot MyBackup is not found in {Path}"));
        }

        [Test]
        public void ImportsBackup()
        {
            CreateSnapshot();

            Execute();

            Assert.That(GetLastSnapshot($"home/snapshots/{BackupName}"), Contains.Substring(HelloHash));
            Assert.That(FS.GetFileAsString($"home/file_storage/18/{HelloHash}"), Is.EqualTo("Hello"));
        }

        private void Execute()
        {
            var command = AutoMock.Create<BackupPullCommand>();

            command.Path = Path;
            command.Name = BackupName;
            command.Date = Date;

            command.Execute(Factories.Config);
        }

        private string GetLastSnapshot(string path)
        {
            var fileName = FS.GetVirtualDirectory(path).Files.Keys.Last();
            return FS.GetFileAsString($"{path}/{fileName}");
        }

        private void CreateSnapshot()
        {
            FS.SetFile($"{Path}/file_storage/18/{HelloHash}", "Hello");
            FS.SetFile($"{Path}/snapshots/{BackupName}/2020-04-25_09-20-30", $"{HelloHash} 2020-01-02T00:00:00.0000000 5 foo/hi.txt\n");
        }
    }
}
