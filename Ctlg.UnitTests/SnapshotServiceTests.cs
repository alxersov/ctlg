using System;
using System.Linq;
using Autofac.Extras.Moq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service;
using Ctlg.Service.Events;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    public class SnapshotServiceTests : BackupTestFixture
    {
        [Test]
        public void FindSnapshotFile_WithoutDate_ReturnsLatest()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var service = CreateService(mock);

                var snapshotPath = service.FindSnapshotFile("Test");
                Assert.That(snapshotPath, Is.EqualTo("X:\\current-directory\\snapshots\\Test\\2019-06-26_00-00-00"));
            }
        }

        [Test]
        public void FindSnapshotFile_WithExactDate()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var service = CreateService(mock);

                var snapshotPath = service.FindSnapshotFile("Test", "2019-01-01T00:00:00");
                Assert.That(snapshotPath, Is.EqualTo("X:\\current-directory\\snapshots\\Test\\2019-01-01_00-00-00"));
            }
        }

        [Test]
        public void FindSnapshotFile_WithDate_ReturnsNearestPrevious()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var service = CreateService(mock);

                var snapshotPath = service.FindSnapshotFile("Test", "2019-01-01T05:00:30");
                Assert.That(snapshotPath, Is.EqualTo("X:\\current-directory\\snapshots\\Test\\2019-01-01_02-30-00"));
            }
        }

        [Test]
        public void ReadSnapshotFile_WhenBadFile_RaisesExceptionEvent()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.SetupOpenFileForRead("some-path", "Bad line");

                var events = SetupEvents<ErrorEvent>();

                var service = CreateService(mock);
                service.ReadSnapshotFile("some-path").ToList();

                Assert.That(events.Count, Is.EqualTo(1));
                Assert.That(events[0].Exception.Message, Does.Contain("Unexpected list line"));
            }
        }

        [Test]
        public void ReadSnapshotFile_WhenCorrectFile_ReturnsSnapshotRecords()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.SetupOpenFileForRead("some-path", FileListLine);

                var service = CreateService(mock);
                var records = service.ReadSnapshotFile("some-path").ToList();

                Assert.That(records.Count, Is.EqualTo(1));
                Assert.That(records[0].ToString(), Is.EqualTo(FileListLine));
            }
        }

        [Test]
        public void GetLastSnapshotPath_WhenSnapshotDoesNotExist_ReturnsNull()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var service = CreateService(mock);
                Assert.That(service.GetLastSnapshotPath("DoesNotExist"), Is.Null);
            }
        }

        [Test]
        public void GetLastSnapshotPath_WhenSnapshotsExist_ReturnsLast()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var service = CreateService(mock);

                Assert.That(service.GetLastSnapshotPath("Test"),
                    Is.EqualTo("X:\\current-directory\\snapshots\\Test\\2019-06-26_00-00-00"));
            }
        }

        [Test]
        public void CreateSnapshotWriter_CreatesWriter()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.SetupHashFunction("SHA-256", null);

                var service = CreateService(mock);
                var stream = mock.SetupCreateNewFileForWrite();

                var writer = service.CreateSnapshotWriter("TestCreateSnapshotWriter");

                Assert.That(writer, Is.Not.Null);
                mock.Mock<IFilesystemService>()
                    .Verify(f => f.CreateDirectory(
                        @"X:\current-directory\snapshots\TestCreateSnapshotWriter"),
                        Times.Once);
                mock.Mock<IFilesystemService>()
                    .Verify(f => f.CreateNewFileForWrite(
                        @"X:\current-directory\snapshots\TestCreateSnapshotWriter\test"),
                        Times.Once);
            }
        }

        private SnapshotService CreateService(AutoMock mock)
        {
            var fs = mock.Mock<IFilesystemService>();

            fs.Setup(f => f.GetCurrentDirectory()).Returns("X:\\current-directory");

            fs
                .Setup(f => f.CombinePath(
                    It.Is<string>(path => path == "X:\\current-directory"),
                    It.Is<string>(path => path == "snapshots")))
                .Returns("X:\\current-directory\\snapshots");

            fs
                .Setup(f => f.CombinePath(
                    It.Is<string>(path => path == "X:\\current-directory\\snapshots"),
                    It.Is<string>(path => path == "Test")))
                .Returns("X:\\current-directory\\snapshots\\Test");

            fs
                .Setup(f => f.CombinePath(
                    It.Is<string>(path => path == @"X:\current-directory\snapshots"),
                    It.Is<string>(path => path == "TestCreateSnapshotWriter")))
                .Returns(@"X:\current-directory\snapshots\TestCreateSnapshotWriter");

            fs
                .Setup(f => f.CombinePath(
                    It.Is<string>(path => path == @"X:\current-directory\snapshots\TestCreateSnapshotWriter"),
                    It.IsAny<string>()))
                .Returns(@"X:\current-directory\snapshots\TestCreateSnapshotWriter\test");

            fs
                .Setup(f => f.DirectoryExists(It.Is<string>(path => path == "X:\\current-directory\\snapshots\\Test")))
                .Returns(true);

            fs
                .Setup(f => f.EnumerateFiles(
                    It.Is<string>(path => path == "X:\\current-directory\\snapshots\\Test"),
                    It.Is<string>(searchMask => searchMask == "????-??-??_??-??-??")))
                .Returns(new[] { new File("2019-01-01_00-00-00"), new File("2019-01-01_02-30-00"), new File("2019-06-26_00-00-00") });

            fs
                .Setup(f => f.CombinePath(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Throws(new Exception("Unexpected path"));

            fs
                .Setup(f => f.CombinePath(
                    It.Is<string>(path => path == "X:\\current-directory\\snapshots"),
                    It.Is<string>(path => path == "Test"),
                    It.Is<string>(path => path == "2019-01-01_00-00-00")))
                .Returns("X:\\current-directory\\snapshots\\Test\\2019-01-01_00-00-00");

            fs
                .Setup(f => f.CombinePath(
                    It.Is<string>(path => path == "X:\\current-directory\\snapshots"),
                    It.Is<string>(path => path == "Test"),
                    It.Is<string>(path => path == "2019-01-01_02-30-00")))
                .Returns("X:\\current-directory\\snapshots\\Test\\2019-01-01_02-30-00");

            fs
                .Setup(f => f.CombinePath(
                    It.Is<string>(path => path == "X:\\current-directory\\snapshots"),
                    It.Is<string>(path => path == "Test"),
                    It.Is<string>(path => path == "2019-06-26_00-00-00")))
                .Returns("X:\\current-directory\\snapshots\\Test\\2019-06-26_00-00-00");

            return mock.Create<SnapshotService>();
        }
    }
}
