using System;
using System.Collections.Generic;
using System.Linq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service;
using Ctlg.Service.Events;
using Ctlg.UnitTests.Fixtures;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests
{
    public class SnapshotTests: AutoMockTestFixture
    {
        private readonly string Hash = "64ec88ca00b268e5ba1a35678a1b5316d212f4f366b2477232534a8aeca37f3c";
        private readonly string FilePath = "1.txt";
        private readonly string FileListLine;
        private readonly string CommentLine = "# Comment";
        private readonly string SnapshotName = "Test";
        private readonly string SnapshotPath = "some/path";
        private readonly string Timestamp = "2019-01-01_02-30-00";


        private Mock<IFilesystemService> FilesystemServiceMock;

        public SnapshotTests()
        {
            FileListLine = $"{Hash} 2018-04-22T18:05:12.0000000Z 11 {FilePath}";
        }

        [SetUp]
        public void Setup()
        {
            FilesystemServiceMock = AutoMock.Mock<IFilesystemService>();
            AutoMock.SetupHashAlgorithm(Factories.HashAlgorithm);
        }

        [Test]
        public void EnumerateFiles_WhenBadFile_RaisesExceptionEvent()
        {
            AutoMock.SetupOpenFileForRead(SnapshotPath, "Bad line");
            var errors = SetupEvents<ErrorEvent>();

            EnumerateFiles();

            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0].Exception.Message, Does.Contain("Unexpected list line"));
        }

        [Test]
        public void EnumerateFiles_WhenCorrectFile_ReturnsSnapshotRecords()
        {
            AutoMock.SetupOpenFileForRead(SnapshotPath, FileListLine);

            var records = EnumerateFiles();

            Assert.That(records.Count, Is.EqualTo(1));
            Assert.That(records[0].Name, Is.EqualTo(FilePath));
        }

        [Test]
        public void EnumerateFiles_FileContainsComment_ReturnsSnapshotRecords()
        {
            AutoMock.SetupOpenFileForRead(SnapshotPath, $"{CommentLine}\n{FileListLine}");
            var errors = SetupEvents<ErrorEvent>();

            var records = EnumerateFiles();

            Assert.That(records.Count, Is.EqualTo(1));
            Assert.That(records[0].Name, Is.EqualTo(FilePath));
            Assert.That(errors.Count, Is.EqualTo(0));
        }

        private TextFileSnapshot CreateSnapshot()
        {
            return new TextFileSnapshot(FilesystemServiceMock.Object, Factories.HashAlgorithm, SnapshotPath, SnapshotName, Timestamp);
        }

        private IList<File> EnumerateFiles()
        {
            var snapshot = CreateSnapshot();

            return snapshot.EnumerateFiles().ToList();
        }
    }
}
