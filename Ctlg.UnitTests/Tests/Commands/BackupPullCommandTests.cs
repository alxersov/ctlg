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
        public readonly string CurrentDir = "home";
        public readonly string Path = "some-path";
        public readonly string Name = "testfoo";
        public readonly string DateToSearch = "2019-01-01";
        public readonly string Timestamp = "2019-01-01_02-02-20";

        public ISnapshot SourceSnapshot;

        public Mock<IBackupService> BackupServiceMock;
        public Mock<IBackupWriter> BackupWriterMock;
        public Mock<IFileStorage> SourceFileStorageMock;

        private File File;

        public BackupPullCommandTests()
        {
            File = new File();
        }

        [SetUp]
        public void Setup()
        {
            Command.Path = Path;
            Command.Name = Name;
            Command.Date = DateToSearch;

            BackupServiceMock = AutoMock.Mock<IBackupService>();

            SnapshotServiceMock.Setup(s => s.GetSnapshot(Path, "SHA-256", Name, DateToSearch))
                .Returns(() => SourceSnapshot);

            SourceFileStorageMock = FileStorageServiceMock.SetupGetFileStorage(Path);

            BackupWriterMock = BackupServiceMock.SetupCreateWriter(CurrentDir, Name, Timestamp);

            SnapshotServiceMock.Setup(s => s.CreateFile(It.IsAny<SnapshotRecord>()))
                .Returns(File);

            SourceSnapshot = Factories.CreateSnapshotMock(Name, Timestamp).Object;
        }

        [Test]
        public void WhenSourceSnapshotNotFound()
        {
            SourceSnapshot = null;

            Assert.That(() => Command.Execute(Factories.Config),
                Throws.TypeOf<Exception>().With.Message.Contain("Snapshot testfoo is not found in some-path"));
        }

        [Test]
        public void WritesBackup()
        {
            Command.Execute(Factories.Config);

            BackupWriterMock.Verify(m => m.AddFile(File, SourceFileStorageMock.Object), Times.Once);
        }
    }
}
