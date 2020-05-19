using System;
using System.Linq;
using Autofac.Extras.Moq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Commands;
using Ctlg.UnitTests.Fixtures;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Commands.Steps
{
    public class SnapshotReaderTests : AutoMockTestFixture
    {
        private File SnapshotRecord;
        private File Root;
        private File File;
        private ISnapshot Snapshot;

        private HashAlgorithm HashAlgorithm;

        [SetUp]
        public void Setup()
        {
            Snapshot = Factories.CreateSnapshotMock("test", "2019_01_01").Object;
            SnapshotRecord = Factories.SnapshotRecords[0];

            Root = new File("root", true);

            File = new File("1.txt")
            {
                Size = SnapshotRecord.Size + 10,
                FileModifiedDateTime = SnapshotRecord.FileModifiedDateTime?.Add(new TimeSpan(1,2,3))
            };

            Root.Contents.Add(File);

            HashAlgorithm = Factories.HashAlgorithm;
            AutoMock.SetupHashAlgorithm(HashAlgorithm);
        }

        [Test]
        public void ReadHashesFromSnapshot_WhenFileNotFound()
        {
            File = null;

            ReadHashes();
        }

        [Test]
        public void ReadHashesFromLatestSnapshot_WhenFileDateAndSizeDontMatch()
        {
            ReadHashes();

            Assert.That(File.Hashes, Is.Empty);
        }

        [Test]
        public void ReadHashesFromLatestSnapshot_WhenFileDateDoesNotMatch()
        {
            File.Size = SnapshotRecord.Size;

            ReadHashes();

            Assert.That(File.Hashes, Is.Empty);
        }

        [Test]
        public void ReadHashesFromLatestSnapshot_WhenFileDateAndSizeMatch()
        {
            File.Size = SnapshotRecord.Size;
            File.FileModifiedDateTime = SnapshotRecord.FileModifiedDateTime;

            ReadHashes();

            Assert.That(File.Hashes.Count, Is.EqualTo(1));
            Assert.That(File.Hashes.First(), Is.EqualTo(SnapshotRecord.Hashes.First()));
        }

        private void ReadHashes()
        {
            var reader = new SnapshotReader();
            reader.ReadHashesFromSnapshot(Snapshot, Root);
        }
    }
}
