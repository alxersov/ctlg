using System;
using System.Linq;
using Ctlg.Service.Commands;
using Ctlg.UnitTests.Fixtures;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Commands
{
    public class BackupCommandTests : CommonDependenciesFixture
    {
        public BackupCommandTests()
        {
        }

        private string HelloHash = "185f8db32271fe25f561a6fc938b2e264306ec304eda518007d1764826381969";
        private string HiHash = "cd08abb273b1619e82e78c29a7df02c1051b1820e99fc395dcaa3326b8";
        private string AppleHash = "f223faa96f22916294922b171a2696d868fd1f9129302eb41a45b2a2ea2ebbfd";
        private string SourcePath = "source";
        private string BackupName = "Backup1";
        private DateTime ModifiedTime = new DateTime(2020, 1, 2);

        [Test]
        public void Adds_file_to_storage_and_snapshot()
        {
            FS.SetFile("source/hi.txt", "Hello", ModifiedTime);

            Execute();

            Assert.That(GetLastSnapshot($"home/snapshots/{BackupName}"), Contains.Substring(HelloHash));
            Assert.That(FS.GetFileAsString($"home/file_storage/18/{HelloHash}"), Is.EqualTo("Hello"));
        }

        [Test]
        public void In_FastMode_date_and_size_is_checked()
        {
            CreateOldHelloSnapshot();
            FS.SetFile("source/hi.txt", "12345", ModifiedTime);

            Execute(true);

            Assert.That(GetLastSnapshot($"home/snapshots/{BackupName}"), Contains.Substring(HelloHash));
        }

        [Test]
        public void In_FastMode_size_is_checked()
        {
            CreateOldHelloSnapshot();
            FS.SetFile("source/hi.txt", "Hi", ModifiedTime);

            Execute(true);

            Assert.That(GetLastSnapshot($"home/snapshots/{BackupName}"), Contains.Substring(HiHash));
        }

        [Test]
        public void In_FastMode_date_is_checked()
        {
            CreateOldHelloSnapshot();
            FS.SetFile("source/hi.txt", "Apple", ModifiedTime.AddDays(1));

            Execute(true);

            Assert.That(GetLastSnapshot($"home/snapshots/{BackupName}"), Contains.Substring(AppleHash));
        }

        private void Execute(bool isFastMode = false)
        {
            var command = AutoMock.Create<BackupCommand>();

            command.Path = SourcePath;
            command.Name = BackupName;
            command.IsFastMode = isFastMode;

            command.Execute(Factories.Config);
        }

        private string GetLastSnapshot(string path)
        {
            var fileName = FS.GetVirtualDirectory(path).Files.Keys.Last();
            return FS.GetFileAsString($"{path}/{fileName}");
        }

        private void CreateOldHelloSnapshot()
        {
            FS.SetFile($"home/file_storage/18/{HelloHash}", "Hello");
            FS.SetFile($"home/snapshots/{BackupName}/2018-01-01_00-00-00", $"{HelloHash} 2020-01-02T00:00:00.0000000 5 hi.txt\n");
        }
    }
}
