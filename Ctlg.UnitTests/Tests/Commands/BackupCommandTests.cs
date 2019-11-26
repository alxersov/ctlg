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
        private readonly string FileName = "test-1.txt";
        private File File;
        private bool IsFastMode;
        private File Tree;

        private Mock<IBackupWriter> BackupWriterMock;
        private Mock<ISnapshotReader> SnapshotReaderMock;

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

            BackupCommandEndedEvents = SetupEvents<BackupCommandEnded>();
        }

        [Test]
        public void Execute_AddsFilesToSnapshot()
        {
            Execute();

            BackupWriterMock.Verify(m => m.AddFile(File), Times.Once);

            Assert.That(BackupCommandEndedEvents.Count, Is.EqualTo(1));

            SnapshotReaderMock.Verify(s => s.ReadHashesFromLatestSnapshot(
                BackupName, Tree), Times.Never);

            IndexFileServiceMock.Verify(s => s.Load(), Times.Once);
            IndexFileServiceMock.Verify(s => s.Save(), Times.Once);

            BackupWriterMock.VerifyAppVersionWritten();
            BackupWriterMock.Verify(m => m.AddComment("FastMode=False"), Times.Once);
        }

        [Test]
        public void Execute_WhenFastMode_ReadsLatestSnapshot()
        {
            IsFastMode = true;

            Execute();

            SnapshotReaderMock.Verify(s => s.ReadHashesFromLatestSnapshot(
                BackupName, Tree), Times.Once);
            BackupWriterMock.Verify(m => m.AddComment("FastMode=True"), Times.Once);
        }

        private void Execute()
        {
            Command.Path = Path;
            Command.Name = BackupName;
            Command.IsFastMode = IsFastMode;

            Command.Execute();
        }

        private void SetupBackupReader()
        {
            SnapshotReaderMock = AutoMock.Mock<ISnapshotReader>();
        }

        private void SetupBackupWriter()
        {
            BackupWriterMock = AutoMock.SetupBackupWriter(BackupName, null,
                shouldUseIndex => shouldUseIndex == IsFastMode, false);
        }

        private void SetupTreeProvider()
        {
            AutoMock.Mock<ITreeProvider>()
                .Setup(d => d.ReadTree(Path, null))
                .Returns(Tree);
        }
    }
}
