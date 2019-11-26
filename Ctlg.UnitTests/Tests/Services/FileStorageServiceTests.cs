using System;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Services;
using Ctlg.UnitTests.Fixtures;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Services
{
    public class FileStorageServiceTests : AutoMockTestFixture
    {
        private Mock<IFilesystemService> FilesystemServiceMock { get; set; }

        private string CurrentDir;
        private readonly string FileStorageDirectory = "some-path/file_storage";
        private readonly string BackupFileDir = "some-path/file_storage/ab";
        private readonly string BackupFilePath = "some-path/file_storage/ab/ab123456";
        private readonly string FullFilePath = "some/full/path/file.ext";
        private readonly Hash Hash = new Hash((int)HashAlgorithmId.SHA256, new byte[] { 0xab, 0x12, 0x34, 0x56 });

        private File File;
        private bool BackedUpFileExists;
        private long BackedUpFileSize;

        private Lazy<FileStorageService> LazySnapshotService;
        private FileStorageService FileStorageService
        {
            get
            {
                return LazySnapshotService.Value;
            }
        }

        [SetUp]
        public void Setup()
        {
            CurrentDir = "some-path";
            File = new File("test")
            {
                FullPath = FullFilePath,
            };
            File.Hashes.Add(Hash);

            BackedUpFileExists = false;
            BackedUpFileSize = 0;

            FilesystemServiceMock = AutoMock.Mock<IFilesystemService>();

            FilesystemServiceMock.Setup(s => s.GetCurrentDirectory()).Returns(CurrentDir);
            FilesystemServiceMock
                .Setup(s => s.CombinePath(CurrentDir, "file_storage"))
                .Returns(FileStorageDirectory);
            FilesystemServiceMock
                .Setup(s => s.CombinePath(FileStorageDirectory, "ab"))
                .Returns(BackupFileDir);
            FilesystemServiceMock
                .Setup(s => s.CombinePath(BackupFileDir, "ab123456"))
                .Returns(BackupFilePath);
            FilesystemServiceMock
                .Setup(s => s.GetDirectoryName(BackupFilePath))
                .Returns(BackupFileDir);
            FilesystemServiceMock
                .Setup(s => s.FileExists(BackupFilePath))
                .Returns(() => BackedUpFileExists);
            FilesystemServiceMock
                .Setup(s => s.GetFileSize(BackupFilePath))
                .Returns(() => BackedUpFileSize);

            LazySnapshotService = new Lazy<FileStorageService>(() => AutoMock.Create<FileStorageService>());
        }

        [Test]
        public void GetBackupFilePath_ReturnsExpectedPath()
        {
            var path = FileStorageService.GetBackupFilePath("ab123456");

            Assert.That(path, Is.EqualTo(BackupFilePath));
        }

        [Test]
        public void GetBackupFilePath_WhenRootPathProvided()
        {
            CurrentDir = "foo";

            var path = FileStorageService.GetBackupFilePath("ab123456", "some-path");

            Assert.That(path, Is.EqualTo(BackupFilePath));
        }

        [Test]
        public void AddFileToStorage()
        {
            FileStorageService.AddFileToStorage(File);

            FilesystemServiceMock.Verify(m => m.CreateDirectory(BackupFileDir), Times.Once);
            FilesystemServiceMock.Verify(m => m.Copy(FullFilePath, BackupFilePath), Times.Once);
        }

        [Test]
        public void FileStorageDirectory_ReturnsCorrectValue()
        {
            Assert.That(FileStorageService.FileStorageDirectory, Is.EqualTo(FileStorageDirectory));
        }

        [Test]
        public void IsFileInStorage_WhenNoFile()
        {
            Assert.That(FileStorageService.IsFileInStorage(File), Is.False);
        }

        [Test]
        public void IsFileInStorage_WhenFileExists()
        {
            BackedUpFileExists = true;

            Assert.That(FileStorageService.IsFileInStorage(File), Is.True);
        }

        [Test]
        public void IsFileInStorage_WhenFileExists_AndSizeInformationMatch()
        {
            BackedUpFileExists = true;
            File.Size = 123;
            BackedUpFileSize = 123;

            Assert.That(FileStorageService.IsFileInStorage(File), Is.True);
        }

        [Test]
        public void IsFileInStorage_WhenFileExists_AndSizeInformationDontMatch()
        {
            BackedUpFileExists = true;
            File.Size = 123;
            BackedUpFileSize = 456;

            Assert.That(() => FileStorageService.IsFileInStorage(File),
                Throws.TypeOf<Exception>().With.Message.Contain("do not match"));
        }
    }
}
