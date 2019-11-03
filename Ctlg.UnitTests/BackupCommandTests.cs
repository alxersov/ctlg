using System;
using System.Collections.Generic;
using Autofac.Extras.Moq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service;
using Ctlg.Service.Commands;
using Ctlg.Service.Events;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    public class BackupCommandTests: AutoMockTestFixture
    {
        private readonly string Path = "test-path";
        private readonly string BackupName = "test-name";
        private readonly string FileName = "test-1.txt";
        private File File;
        private bool IsFastMode;
        private File Tree;

        private Mock<IBackupWriter> BackupWriterMock;
        private Mock<ISnapshotReader> SnapshotReaderMock;
        private Mock<IIndexFileService> IndexFileServiceMock;

        private IList<BackupCommandEnded> BackupCommandEndedEvents;

        [SetUp]
        public void Init()
        {
            IsFastMode = false;
            File = new File(FileName);
            Tree = new File(Path, true) { Contents = { File } };

            SetupTreeProvider();
            SetupBackupReader();
            SetupBackupWriter();
            SetupIndexFileService();

            BackupCommandEndedEvents = SetupEvents<BackupCommandEnded>();
        }

        [Test]
        public void Execute_AddsFilesToSnapshot()
        {
            Execute();

            BackupWriterMock.VerifyAll();

            Assert.That(BackupCommandEndedEvents.Count, Is.EqualTo(1));

            SnapshotReaderMock.Verify(s => s.ReadHashesFromLatestSnapshot(
                BackupName, Tree), Times.Never);

            IndexFileServiceMock.Verify(s => s.Load(), Times.Once);
            IndexFileServiceMock.Verify(s => s.Save(), Times.Once);
        }

        [Test]
        public void Execute_WhenFastMode_ReadsLatestSnapshot()
        {
            IsFastMode = true;

            Execute();

            SnapshotReaderMock.Verify(s => s.ReadHashesFromLatestSnapshot(
                BackupName, Tree), Times.Once);
        }

        private void Execute()
        {
            var command = AutoMock.Create<BackupCommand>();
            command.Path = Path;
            command.Name = BackupName;
            command.IsFastMode = IsFastMode;

            command.Execute();
        }

        private void SetupBackupReader()
        {
            SnapshotReaderMock = AutoMock.Mock<ISnapshotReader>();
        }

        private void SetupBackupWriter()
        {
            BackupWriterMock = new Mock<IBackupWriter>();
            BackupWriterMock.Setup(w => w.AddFile(File));

            AutoMock.Mock<ICtlgService>()
                .Setup(p => p.CreateBackupWriter(
                    BackupName, It.Is<bool>(shouldUseIndex => shouldUseIndex == IsFastMode)))
                .Returns(BackupWriterMock.Object);
        }

        private void SetupTreeProvider()
        {
            AutoMock.Mock<ITreeProvider>()
                .Setup(d => d.ReadTree(Path, null))
                .Returns(Tree);
        }

        private void SetupIndexFileService()
        {
            IndexFileServiceMock = AutoMock.Mock<IIndexFileService>();
        }
    }
}
