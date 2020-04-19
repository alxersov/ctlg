using System;
using Autofac.Extras.Moq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Services;
using Ctlg.UnitTests.Fixtures;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Services
{
    public class SnapshotServiceTests : AutoMockTestFixture
    {
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

        private SnapshotService SnapshotService;

        private Mock<IFilesystemService> FilesystemServiceMock;

        public SnapshotServiceTests()
        {
            TestSnapshotsDirectory = $@"{RootDirectory}\{AllSnapnsotsDirectory}\{SnapshotName}";
            SnapshotFile1 = $@"{TestSnapshotsDirectory}\{Date1}";
            SnapshotFile2 = $@"{TestSnapshotsDirectory}\{Date2}";
            SnapshotFile3 = $@"{TestSnapshotsDirectory}\{Date3}";
        }

        [SetUp]
        public void Setup()
        {
            SetupMocks();
            SnapshotService = AutoMock.Create<SnapshotService>();
        }

        [Test]
        public void GetsLatesSnapshot()
        {
            var snapshot = SnapshotService.GetSnapshot(RootDirectory, "SHA-256", SnapshotName, null);
            Assert.That(snapshot.Timestamp, Is.EqualTo(Date3));
        }

        [Test]
        public void GetSnapshot_WhenSnapshotDoesNotExist_ReturnsNull()
        {
            var snapshot = SnapshotService.GetSnapshot(RootDirectory, "SHA-256", "DoesNotExist", null);
            Assert.That(snapshot, Is.Null);
        }

        [Test]
        public void GetSnapshot_WhenNoSnapshotMatchesProvidedDate_ReturnsNull()
        {
            var snapshot = SnapshotService.GetSnapshot(RootDirectory, "SHA-256", SnapshotName, "2019-09-03");
            Assert.That(snapshot, Is.Null);
        }

        [Test]
        public void GetSnapshot_WithExactDate()
        {
            var snapshot = SnapshotService.GetSnapshot(RootDirectory, "SHA-256", SnapshotName, Date1);
            Assert.That(snapshot.Timestamp, Is.EqualTo(Date1));
        }

        [Test]
        public void GetSnapshot_WithOneDateMatching()
        {
            var snapshot = SnapshotService.GetSnapshot(RootDirectory, "SHA-256", SnapshotName, "2019-01-01_02");
            Assert.That(snapshot.Timestamp, Is.EqualTo(Date2));
        }

        [Test]
        public void GetSnapshot_WithMoreThanOneDateMatching()
        {
            Assert.That(() => SnapshotService.GetSnapshot(RootDirectory, "SHA-256", SnapshotName, "2019-01-01"),
                Throws.InstanceOf<Exception>().With.Message.Contain("date is ambiguous"));
        }

        [Test]
        public void CreateSnapshot_WhenTimestampIsNotSpecified()
        {
            FilesystemServiceMock
                .Setup(f => f.CombinePath(TestSnapshotsDirectory, It.IsAny<string>()))
                .Returns(SnapshotFile1);

            var snapshot = SnapshotService.CreateSnapshot(RootDirectory, "SHA-256", SnapshotName, null);

            Assert.That(snapshot.Timestamp, Is.Not.Null);
            Assert.That(snapshot.Name, Is.EqualTo(SnapshotName));
            FilesystemServiceMock.Verify(m => m.CombinePath(TestSnapshotsDirectory, snapshot.Timestamp));
        }

        [Test]
        public void CreateSnapshot_WhenTimestampIsSpecified()
        {
            var snapshot = SnapshotService.CreateSnapshot(RootDirectory, "SHA-256", SnapshotName, Date2);

            Assert.That(snapshot.Timestamp, Is.EqualTo(Date2));
        }

        private void SetupMocks()
        {
            FilesystemServiceMock = AutoMock.Mock<IFilesystemService>();

            FilesystemServiceMock.SetupPath(RootDirectory, AllSnapnsotsDirectory, SnapshotName, TestSnapshotsDirectory);
            FilesystemServiceMock.SetupPath(TestSnapshotsDirectory, Date1, SnapshotFile1);
            FilesystemServiceMock.SetupPath(TestSnapshotsDirectory, Date2, SnapshotFile2);
            FilesystemServiceMock.SetupPath(TestSnapshotsDirectory, Date3, SnapshotFile3);

            FilesystemServiceMock
                .Setup(f => f.DirectoryExists(TestSnapshotsDirectory))
                .Returns(true);

            FilesystemServiceMock
                .Setup(f => f.EnumerateFiles(TestSnapshotsDirectory, "????-??-??_??-??-??"))
                .Returns(new[] { new File(Date1), new File(Date2), new File(Date3) });
        }
    }
}
