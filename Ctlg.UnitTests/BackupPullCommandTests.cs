using System;
using Ctlg.Core;
using File = Ctlg.Core.File;
using Ctlg.Service.Commands;
using NUnit.Framework;
using Moq;
using Ctlg.Core.Interfaces;

namespace Ctlg.UnitTests
{
    public class BackupPullCommandTests: CommandTestFixture<BackupPullCommand>
    {
        public readonly string Path = "some-path";
        public readonly string Name = "testfoo";
        public readonly string DateToSearch = "2019-01-01";

        public readonly SnapshotFile SnapshotFile;
        private SnapshotRecord SnapshotRecord;

        private File File;
        private string FileFullPath = "file/full/path";
        private Mock<IBackupWriter> BackupWriterMock;

        public BackupPullCommandTests()
        {
            SnapshotFile = Factories.CreateSnapshotFile();
            SnapshotRecord = Factories.SnapshotRecords[0];
        }

        [SetUp]
        public void Setup()
        {
            Command.Path = Path;
            Command.Name = Name;
            Command.Date = DateToSearch;
 
            File = new File();

            SnapshotServiceMock.Setup(s => s.FindSnapshotFile(Path, Name, DateToSearch)).Returns(SnapshotFile);
            SnapshotServiceMock.Setup(s => s.ReadSnapshotFile(SnapshotFile.FullPath)).Returns(new[] { SnapshotRecord });
            SnapshotServiceMock.Setup(s => s.CreateFile(SnapshotRecord)).Returns(File);

            BackupWriterMock = AutoMock.SetupBackupWriter(SnapshotFile.Name, SnapshotFile.Date,
                shouldUseIndex => shouldUseIndex == false, true);

            FileStorageServiceMock.Setup(s => s.GetBackupFilePath(SnapshotRecord.Hash, Path)).Returns(FileFullPath);
        }

        [Test]
        public void Execute_WhenFileIsAlreadyInStorage()
        {
            Command.Execute();

            Assert.That(File.FullPath, Is.EqualTo(FileFullPath));
            BackupWriterMock.Verify(m => m.AddFile(File), Times.Once);
        }

    }
}
