using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.Moq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Commands;
using Ctlg.Service.Utils;
using Ctlg.UnitTests.Fixtures;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Commands.Steps
{
    public class SnapshotReaderTests : AutoMockTestFixture
    {
        private Mock<ISnapshotService> SnapshotServiceMock;
        private Mock<ICtlgService> CtlgServiceMock;
        private string SnapshotPath;
        private SnapshotRecord SnapshotRecord;
        private IEnumerable<SnapshotRecord> SnapshotRecords
        {
            get { yield return SnapshotRecord; }
        }
        private File Root;
        private File File;
        private ISnapshot Snapshot;

        [SetUp]
        public void Setup()
        {
            Snapshot = Factories.CreateSnapshotMock("test", "2019_01_01").Object;
            SnapshotRecord = Factories.SnapshotRecords[0];

            Root = new File("root", true);

            File = new File("foo")
            {
                Size = SnapshotRecord.Size + 10,
                FileModifiedDateTime = SnapshotRecord.Date.Add(new TimeSpan(1,2,3))
            };

            CtlgServiceMock = AutoMock.Mock<ICtlgService>();

            CtlgServiceMock.Setup(s => s.GetInnerFile(
                It.IsAny<File>(), It.IsAny<string>()))
                .Returns(() => File);
        }

        [Test]
        public void ReadHashesFromSnapshot_WhenFileNotFound()
        {
            File = null;

            ReadHashes();

            CtlgServiceMock.Verify(s => s.SortTree(Root), Times.Once);
            CtlgServiceMock.Verify(s => s.GetInnerFile(Root, SnapshotRecord.Name), Times.Once);
        }

        [Test]
        public void ReadHashesFromLatestSnapshot_WhenFileDateAndSizeDontMatch()
        {
            ReadHashes();

            CtlgServiceMock.Verify(s => s.SortTree(Root), Times.Once);
            CtlgServiceMock.Verify(s => s.GetInnerFile(Root, SnapshotRecord.Name), Times.Once);

            Assert.That(File.Hashes, Is.Empty);
        }

        [Test]
        public void ReadHashesFromLatestSnapshot_WhenFileDateDoesNotMatch()
        {
            File.Size = SnapshotRecord.Size;

            ReadHashes();

            CtlgServiceMock.Verify(s => s.SortTree(Root), Times.Once);
            CtlgServiceMock.Verify(s => s.GetInnerFile(Root, SnapshotRecord.Name), Times.Once);

            Assert.That(File.Hashes, Is.Empty);
        }

        [Test]
        public void ReadHashesFromLatestSnapshot_WhenFileDateAndSizeMatch()
        {
            File.Size = SnapshotRecord.Size;
            File.FileModifiedDateTime = SnapshotRecord.Date;

            ReadHashes();

            Assert.That(File.Hashes.Count, Is.EqualTo(1));
            Assert.That(File.Hashes.First, Is.EqualTo(new Hash(
                HashAlgorithmId.SHA256, FormatBytes.ToByteArray(SnapshotRecord.Hash))));
        }

        private void ReadHashes()
        {
            var reader = AutoMock.Create<SnapshotReader>();
            reader.ReadHashesFromSnapshot(Snapshot, Root);
        }
    }
}
