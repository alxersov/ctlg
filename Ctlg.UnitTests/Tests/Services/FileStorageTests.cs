using System;
using Autofac;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service;
using Ctlg.Service.Utils;
using Ctlg.UnitTests.Fixtures;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Services
{
    public class FileStorageTests: AutoMockTestFixture
    {
        private readonly string BackupRoot = "backup_root";
        private bool ShouldUseIndex;
        private bool ShouldExistingHashMatchCaclulated;
        private readonly string FileStorageDirectory = "some-path/file_storage";
        private readonly string BackupFileDir = "some-path/file_storage/ab";
        private readonly string BackupFilePath = "some-path/file_storage/ab/ab123456";
        private readonly string FullFilePath = "some/full/path/file.ext";
        private readonly Hash Hash1 = new Hash(HashAlgorithmId.SHA256, new byte[] { 3, 4, 5, 6 });
        private readonly Hash Hash2 = new Hash((int)HashAlgorithmId.SHA256, new byte[] { 0xab, 0x12, 0x34, 0x56 });

        private bool BackedUpFileExists;
        private long BackedUpFileSize;
        private File File;

        private Mock<IFilesystemService> FilesystemServiceMock { get; set; }
        private Mock<ICtlgService> CtlgServiceMock;
        private Mock<IFileStorageIndex> IndexMock;

        [SetUp]
        public void SetUp()
        {
            ShouldUseIndex = false;
            ShouldExistingHashMatchCaclulated = false;

            File = new File("test")
            {
                FullPath = FullFilePath,
            };

            BackedUpFileExists = false;
            BackedUpFileSize = 0;

            IndexMock = new Mock<IFileStorageIndex>();

            var indexServiceMock = AutoMock.Mock<IFileStorageIndexService>();
            indexServiceMock.Setup(s => s.GetIndex(BackupRoot)).Returns(IndexMock.Object);

            IndexMock.Setup(m => m.IsInIndex(Hash2.Value)).Returns(true);

            FilesystemServiceMock = AutoMock.Mock<IFilesystemService>();

            FilesystemServiceMock.Setup(s => s.GetCurrentDirectory()).Returns(BackupRoot);

            FilesystemServiceMock.SetupPath(BackupRoot, "file_storage", FileStorageDirectory);
            FilesystemServiceMock.SetupPath(FileStorageDirectory, "ab", BackupFileDir);
            FilesystemServiceMock.SetupPath(BackupFileDir, "ab123456", BackupFilePath);

            FilesystemServiceMock
                .Setup(s => s.FileExists(BackupFilePath))
                .Returns(() => BackedUpFileExists);
            FilesystemServiceMock
                .Setup(s => s.GetFileSize(BackupFilePath))
                .Returns(() => BackedUpFileSize);

            CtlgServiceMock = AutoMock.Mock<ICtlgService>();
            CtlgServiceMock.Setup(s => s.CalculateHashForFile(File, It.IsAny<IHashFunction>()))
                .Callback<File, IHashFunction>(CalculateHashForFileImpl)
                .Returns(Hash2);
        }

        [Test]
        public void AddsFileIfItIsNotInStorage()
        {
            var status = AddFileToStorage();

            Assert.That(status.IsNotFound(), Is.True);
            FilesystemServiceMock.Verify(m => m.Copy(FullFilePath, BackupFilePath), Times.Once);
            IndexMock.Verify(m => m.Add(Hash2.Value));
            IndexMock.Verify(m => m.Load(), Times.Once);
            IndexMock.Verify(m => m.Save(), Times.Once);
        }

        [Test]
        public void WhenExistingHashDoesNotMatchCalculated()
        {
            File.Hashes.Add(Hash1);

            var status = AddFileToStorage();

            Assert.That(status.IsNotFound(), Is.True);
            FilesystemServiceMock.Verify(m => m.Copy(FullFilePath, BackupFilePath), Times.Once);
            IndexMock.Verify(m => m.Add(Hash2.Value));
            IndexMock.Verify(m => m.Load(), Times.Once);
            IndexMock.Verify(m => m.Save(), Times.Once);
        }

        [Test]
        public void ChecksHashes()
        {
            File.Hashes.Add(Hash1);
            ShouldExistingHashMatchCaclulated = true;

            Assert.That(() => AddFileToStorage(), Throws.InstanceOf<Exception>()
                .With.Message.Contain("Caclulated hash does not match expected for file some/full/path/file.ext."));
        }

        [Test]
        public void WhenFileIsInStorage()
        {
            BackedUpFileExists = true;
            File.Size = BackedUpFileSize = 123;

            var status = AddFileToStorage();

            Assert.That(status.HasFlag(BackupFileStatus.FoundInStorage), Is.True);
            FilesystemServiceMock.VerifyCopyNeverCalled();
        }

        [Test]
        public void ChecksFileSize()
        {
            BackedUpFileExists = true;
            BackedUpFileSize = 123;
            File.Size = 456;

            Assert.That(() => AddFileToStorage(), Throws.InstanceOf<Exception>()
                .With.Message.Contain("The size of \"some/full/path/file.ext\" and \"some-path/file_storage/ab/ab123456\" do not match"));
        }

        [Test]
        public void WhenFileIsInIndex()
        {
            ShouldUseIndex = true;

            var status = AddFileToStorage();

            Assert.That(status.HasFlag(BackupFileStatus.FoundInIndex), Is.True);
            FilesystemServiceMock.VerifyCopyNeverCalled();
        }

        private BackupFileStatus AddFileToStorage()
        {
            BackupFileStatus status;

            using (var storage = GetFileStorage())
            {
                status = storage.AddFileToStorage(File);
            }

            return status;
        }

        private FileStorage GetFileStorage()
        {
            NamedParameter[] parameters =
            {
                new NamedParameter("backupRoot", BackupRoot),
                new NamedParameter("shouldUseIndex", ShouldUseIndex),
                new NamedParameter("shouldExistingHashMatchCaclulated", ShouldExistingHashMatchCaclulated)
            };

            return AutoMock.Create<FileStorage>(parameters);
        }

        private void CalculateHashForFileImpl(File f, IHashFunction hashFunction)
        {
            f.Hashes.Add(Hash2);
        }
    }
}
