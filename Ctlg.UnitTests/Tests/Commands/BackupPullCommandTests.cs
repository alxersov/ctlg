using System;
using Ctlg.Core;
using File = Ctlg.Core.File;
using Ctlg.Service.Commands;
using NUnit.Framework;
using Moq;
using Ctlg.Core.Interfaces;
using Ctlg.UnitTests.Fixtures;

namespace Ctlg.UnitTests.Tests.Commands
{
    public class BackupPullCommandTests: CommandTestFixture<BackupPullCommand>
    {
        public readonly string Path = "some-path";
        public readonly string Name = "testfoo";
        public readonly string DateToSearch = "2019-01-01";

        private SnapshotFile SnapshotFile;
        private SnapshotRecord SnapshotRecord;

        private File File;
        private string FileFullPath = "file/full/path";
        private Mock<IBackupWriter> BackupWriterMock;


        public BackupPullCommandTests()
        {
            SnapshotRecord = Factories.SnapshotRecords[0];
        }

        [SetUp]
        public void Setup()
        {
            Command.Path = Path;
            Command.Name = Name;
            Command.Date = DateToSearch;
 
            File = new File();
            SnapshotFile = Factories.CreateSnapshotFile();

            SnapshotServiceMock.Setup(s => s.FindSnapshotFile(Path, Name, DateToSearch)).Returns(() => SnapshotFile);
            SnapshotServiceMock.Setup(s => s.ReadSnapshotFile(SnapshotFile.FullPath)).Returns(new[] { SnapshotRecord });
            SnapshotServiceMock.Setup(s => s.CreateFile(SnapshotRecord)).Returns(File);

            BackupWriterMock = AutoMock.SetupBackupWriter(SnapshotFile.Name, SnapshotFile.Date,
                shouldUseIndex => shouldUseIndex == false, true);

            FileStorageServiceMock.Setup(s => s.GetBackupFilePath(SnapshotRecord.Hash, Path)).Returns(FileFullPath);
        }

        [Test]
        public void Execute_AddsFilesToBackup()
        {
            Command.Execute();

            Assert.That(File.FullPath, Is.EqualTo(FileFullPath));
            BackupWriterMock.Verify(m => m.AddFile(File), Times.Once);

            IndexFileServiceMock.Verify(s => s.Load(), Times.Once);
            IndexFileServiceMock.Verify(s => s.Save(), Times.Once);

            BackupWriterMock.VerifyAppVersionWritten();
            BackupWriterMock.Verify(m => m.AddComment("Created with pull-backup command."), Times.Once);
        }

        [Test]
        public void Execute_WhenSnapshotFileIsNotFound()
        {
            SnapshotFile = null;

            Assert.That(() => Command.Execute(),
                Throws.TypeOf<Exception>().With.Message.Contain("Snapshot testfoo is not found in some-path"));
        }
    }
}
