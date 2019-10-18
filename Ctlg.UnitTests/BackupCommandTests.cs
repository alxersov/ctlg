using System;
using Autofac.Extras.Moq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Commands;
using Ctlg.Service.Events;
using Ctlg.Service.Utils;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    public class BackupCommandTests: BackupTestFixture
    {
        [Test]
        public void Execute_AddsFilesToSnapshot()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var events = SetupEvents<BackupCommandEnded>();

                SetupTreeProvider(mock);
                var writerMock = SetupSnapshotWriter(mock);

                var command = mock.Create<BackupCommand>();
                command.Path = "test-path";
                command.Name = "test-name";

                command.Execute();

                writerMock.VerifyAll();

                Assert.That(events.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void Execute_WhenFastMode_ReadsLatesSnapshot()
        {
            using (var mock = AutoMock.GetLoose())
            {
                SetupTreeProvider(mock);
                SetupSnapshotWriter(mock);

                var snapshotReaderMock = mock.Mock<ISnapshotReader>();
                snapshotReaderMock.Setup(r => r.ReadHashesFromLatestSnapshot(It.Is<string>(s => s == "test-name"),
                        It.Is<File>(f => f == Tree)));

                var command = mock.Create<BackupCommand>();
                command.Path = "test-path";
                command.Name = "test-name";
                command.IsFastMode = true;

                command.Execute();

                snapshotReaderMock.VerifyAll();
            }
        }

        [SetUp]
        public void Init()
        {
            Tree = new File("test-path", true) { Contents = { new File("test-1.txt") } };
        }

        private File Tree;

        private static Mock<ISnapshotWriter> SetupSnapshotWriter(AutoMock mock)
        {
            var writerMock = new Mock<ISnapshotWriter>();
            writerMock.Setup(w => w.AddFile(It.Is<File>(file => file.Name == "test-1.txt")));

            mock.Mock<ISnapshotService>()
                .Setup(p => p.CreateSnapshotWriter(It.Is<string>(name => name == "test-name")))
                .Returns(writerMock.Object);

            return writerMock;
        }

        private void SetupTreeProvider(AutoMock mock)
        {
            mock.Mock<ITreeProvider>()
                .Setup(d => d.ReadTree(It.Is<string>(s => s == "test-path"), It.Is<string>(s => s == null)))
                .Returns(Tree);
        }
    }
}
