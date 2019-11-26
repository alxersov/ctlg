using System;
using System.Linq;
using Autofac.Extras.Moq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;
using Ctlg.Service.Services;
using Ctlg.UnitTests.Fixtures;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Services
{
    public class SnapshotServiceTests : AutoMockTestFixture
    {
        private readonly string Hash = "64ec88ca00b268e5ba1a35678a1b5316d212f4f366b2477232534a8aeca37f3c";
        private readonly string FilePath = "1.txt";
        private readonly string FileListLine;
        private readonly string CommentLine = "# Comment";
        private readonly string RootDirectory = "X:\\some-directory";
        private readonly string Date1 = "2019-01-01_00-00-00";
        private readonly string Date2 = "2019-01-01_02-30-00";
        private readonly string Date3 = "2019-06-26_00-00-00";
        private readonly string AllSnapnsotsDirectory = "snapshots";
        private readonly string TestSnapshotsDirectory;
        private readonly string SnapshotFile1;
        private readonly string SnapshotFile2;
        private readonly string SnapshotFile3;
        private readonly string SnapshotName = "Test";
        private string CurrentDir;

        private Lazy<SnapshotService> LazySnapshotService;
        private SnapshotService SnapshotService
        {
            get
            {
                return LazySnapshotService.Value;
            }
        }

        private Mock<IFilesystemService> FilesystemServiceMock;

        public SnapshotServiceTests()
        {
            TestSnapshotsDirectory = $@"{RootDirectory}\{AllSnapnsotsDirectory}\{SnapshotName}";
            SnapshotFile1 = $@"{TestSnapshotsDirectory}\{Date1}";
            SnapshotFile2 = $@"{TestSnapshotsDirectory}\{Date2}";
            SnapshotFile3 = $@"{TestSnapshotsDirectory}\{Date3}";
            FileListLine = $"{Hash} 2018-04-22T18:05:12.0000000Z 11 {FilePath}";
        }

        [SetUp]
        public void Setup()
        {
            CurrentDir = RootDirectory;

            SetupMocks();

            LazySnapshotService = new Lazy<SnapshotService>(() => AutoMock.Create<SnapshotService>());
        }

        [Test]
        public void FindSnapshotPath_WithoutDate_ReturnsLatest()
        {
            var snapshotPath = SnapshotService.FindSnapshotPath(SnapshotName);
            Assert.That(snapshotPath, Is.EqualTo(SnapshotFile3));
        }

        [Test]
        public void FindSnapshotPath_WithExactDate()
        {
            var snapshotPath = SnapshotService.FindSnapshotPath(SnapshotName, Date1);
            Assert.That(snapshotPath, Is.EqualTo(SnapshotFile1));
        }

        [Test]
        public void FindSnapshotPath_WithOneDateMatching()
        {
            var snapshotPath = SnapshotService.FindSnapshotPath(SnapshotName, "2019-01-01_02");
            Assert.That(snapshotPath, Is.EqualTo(SnapshotFile2));
        }

        [Test]
        public void FindSnapshotPath_WithMoreThanOneDateMatching()
        {
            Assert.That(() => SnapshotService.FindSnapshotPath(SnapshotName, "2019-01-01"),
                Throws.InstanceOf<Exception>().With.Message.Contain("date is ambiguous"));
        }

        [Test]
        public void FindSnapshotPath_WhenNoSnapshotMatchesProvidedDate_ReturnsNull()
        {
            var snapshotPath = SnapshotService.FindSnapshotPath(SnapshotName, "2019-09-03");
            Assert.That(snapshotPath, Is.Null);
        }

        [Test]
        public void FindSnapshotPath_WhenSnapshotDoesNotExist_ReturnsNull()
        {
            var snapshotPath = SnapshotService.FindSnapshotPath("DoesNotExist");
            Assert.That(snapshotPath, Is.Null);
        }

        [Test]
        public void FindSnapshotFile_WhenRootDirectoyIsNotCurrent()
        {
            CurrentDir = "another-dir";

            var snapshotFile = SnapshotService.FindSnapshotFile(RootDirectory, SnapshotName, Date1);
            Assert.That(snapshotFile.FullPath, Is.EqualTo(SnapshotFile1));
            Assert.That(snapshotFile.Name, Is.EqualTo(SnapshotName));
            Assert.That(snapshotFile.Date, Is.EqualTo(Date1));
        }

        [Test]
        public void ReadSnapshotFile_WhenBadFile_RaisesExceptionEvent()
        {
            AutoMock.SetupOpenFileForRead(SnapshotFile3, "Bad line");

            var events = SetupEvents<ErrorEvent>();

            SnapshotService.ReadSnapshotFile(SnapshotFile3).ToList();

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Exception.Message, Does.Contain("Unexpected list line"));
        }

        [Test]
        public void ReadSnapshotFile_WhenCorrectFile_ReturnsSnapshotRecords()
        {
            AutoMock.SetupOpenFileForRead(SnapshotFile3, FileListLine);

            var records = SnapshotService.ReadSnapshotFile(SnapshotFile3).ToList();

            Assert.That(records.Count, Is.EqualTo(1));
            Assert.That(records[0].ToString(), Is.EqualTo(FileListLine));
        }

        [Test]
        public void ReadSnapshotFile_FileContainsComment_ReturnsSnapshotRecords()
        {
            AutoMock.SetupOpenFileForRead(SnapshotFile3, $"{CommentLine}\n{FileListLine}");
            var errors = SetupEvents<ErrorEvent>();

            var records = SnapshotService.ReadSnapshotFile(SnapshotFile3).ToList();

            Assert.That(records.Count, Is.EqualTo(1));
            Assert.That(records[0].ToString(), Is.EqualTo(FileListLine));
            Assert.That(errors.Count, Is.EqualTo(0));
        }

        [Test]
        public void CreateSnapshotWriter_CreatesWriter()
        {
            var stream = AutoMock.SetupCreateNewFileForWrite();

            FilesystemServiceMock
                .Setup(f => f.CombinePath(TestSnapshotsDirectory, It.IsAny<string>()))
                .Returns(SnapshotFile1);

            var writer = SnapshotService.CreateSnapshotWriter(SnapshotName);

            Assert.That(writer, Is.Not.Null);
            FilesystemServiceMock.Verify(f => f.CreateDirectory(TestSnapshotsDirectory), Times.Once);
            FilesystemServiceMock.Verify(f => f.CreateNewFileForWrite(SnapshotFile1), Times.Once);
        }

        [Test]
        public void CreateSnapshotWriter_WhenTimestampIsSpecified()
        {
            var stream = AutoMock.SetupCreateNewFileForWrite();

            var writer = SnapshotService.CreateSnapshotWriter(SnapshotName, Date2);

            Assert.That(writer, Is.Not.Null);
            FilesystemServiceMock.Verify(f => f.CreateDirectory(TestSnapshotsDirectory), Times.Once);
            FilesystemServiceMock.Verify(f => f.CreateNewFileForWrite(SnapshotFile2), Times.Once);
        }

        [Test]
        public void CreateFile_ReturnsNewFile()
        {
            var record = new SnapshotRecord(FileListLine);

            var file = SnapshotService.CreateFile(record);
            var fileHash = file.Hashes.First(h => h.HashAlgorithmId == (int)HashAlgorithmId.SHA256);

            Assert.That(file.Name, Is.EqualTo(record.Name));
            Assert.That(file.Size, Is.EqualTo(record.Size));
            Assert.That(file.FileModifiedDateTime, Is.EqualTo(record.Date));
            Assert.That(fileHash.ToString(), Is.EqualTo(record.Hash));
        }

        [Test]
        public void CreateSnapshotRecord_RetursSnapshotRecord()
        {
            var sourceRecord = new SnapshotRecord(FileListLine);
            var file = SnapshotService.CreateFile(sourceRecord);

            var destRecord = SnapshotService.CreateSnapshotRecord(file);

            Assert.That(destRecord.ToString(), Is.EqualTo(sourceRecord.ToString()));
        }

        private void SetupMocks()
        {
            FilesystemServiceMock = AutoMock.Mock<IFilesystemService>();

            FilesystemServiceMock.Setup(f => f.GetCurrentDirectory()).Returns(() => CurrentDir);

            FilesystemServiceMock
                .Setup(f => f.CombinePath(RootDirectory, AllSnapnsotsDirectory, SnapshotName))
                .Returns(TestSnapshotsDirectory);

            FilesystemServiceMock
                .Setup(f => f.DirectoryExists(TestSnapshotsDirectory))
                .Returns(true);

            FilesystemServiceMock
                .Setup(f => f.EnumerateFiles(TestSnapshotsDirectory, "????-??-??_??-??-??"))
                .Returns(new[] { new File(Date1), new File(Date2), new File(Date3) });

            FilesystemServiceMock
                .Setup(f => f.CombinePath(TestSnapshotsDirectory, Date1)).Returns(SnapshotFile1);

            FilesystemServiceMock
                .Setup(f => f.CombinePath(TestSnapshotsDirectory, Date2)).Returns(SnapshotFile2);

            FilesystemServiceMock
                .Setup(f => f.CombinePath(TestSnapshotsDirectory, Date3)).Returns(SnapshotFile3);
        }
    }
}
