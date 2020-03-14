using System;
using System.Collections.Generic;
using Autofac.Extras.Moq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Commands;
using Ctlg.Service.Events;
using Ctlg.UnitTests.Fixtures;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Commands
{
    public class BackupCommandTests: CommandTestFixture<BackupCommand>
    {
        private readonly string Path = "test-path";
        private readonly string BackupName = "test-name";
        private readonly string CurrentDirectory = "current-dir";
        private readonly string FileName = "test-1.txt";
        private File File;
        private bool IsFastMode;
        private File Tree;

        private Mock<ISnapshotReader> SnapshotReaderMock;

        private Mock<IBackupService> BackupServiceMock;
        private Mock<IBackupWriter> BackupWriterMock;

        private IList<BackupCommandEnded> BackupCommandEndedEvents;

        [SetUp]
        public void Init()
        {
            IsFastMode = false;
            File = new File(FileName);
            Tree = new File(Path, true) { Contents = { File } };

            SetupTreeProvider();
            SetupSnapshotReader();

            BackupServiceMock = AutoMock.Mock<IBackupService>();
            BackupWriterMock = BackupServiceMock.SetupCreateWriter(CurrentDirectory, BackupName, null);

            FilesystemServiceMock.Setup(s => s.GetCurrentDirectory()).Returns(CurrentDirectory);

            SnapshotServiceMock
                .Setup(s => s.GetSnapshot(CurrentDirectory, BackupName, null))
                .Returns(new Mock<ISnapshot>().Object);

            BackupCommandEndedEvents = SetupEvents<BackupCommandEnded>();
        }

        [Test]
        public void AddsFilesToBackup()
        {
            Execute();

            BackupWriterMock.Verify(m => m.AddFile(File, null), Times.Once);
            BackupWriterMock.VerifyAppVersionWritten();
            BackupWriterMock.Verify(m => m.AddComment("FastMode=False"), Times.Once);

            Assert.That(BackupCommandEndedEvents.Count, Is.EqualTo(1));
            SnapshotReaderMock.Verify(s => s.ReadHashesFromSnapshot(
                It.IsAny<ISnapshot>(), Tree), Times.Never);
        }

        [Test]
        public void Execute_WhenFastMode_ReadsLatestSnapshot()
        {
            IsFastMode = true;

            Execute();

            SnapshotReaderMock.Verify(s => s.ReadHashesFromSnapshot(
                It.IsAny<ISnapshot>(), Tree), Times.Once);
            BackupWriterMock.Verify(m => m.AddComment("FastMode=True"), Times.Once);
        }

        private void Execute()
        {
            Command.Path = Path;
            Command.Name = BackupName;
            Command.IsFastMode = IsFastMode;

            Command.Execute();
        }

        private void SetupSnapshotReader()
        {
            SnapshotReaderMock = AutoMock.Mock<ISnapshotReader>();
        }

        private void SetupTreeProvider()
        {
            AutoMock.Mock<ITreeProvider>()
                .Setup(d => d.ReadTree(Path, null))
                .Returns(Tree);
        }
    }
}
