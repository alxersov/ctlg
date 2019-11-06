using System;
using System.IO;
using Autofac;
using File = Ctlg.Core.File;
using Ctlg.Service;
using NUnit.Framework;
using Moq;
using Ctlg.Core;
using System.Linq;
using Ctlg.Core.Interfaces;
using System.Collections.Generic;
using Ctlg.Service.Events;

namespace Ctlg.UnitTests
{
    public class BackupWriterTests : AutoMockTestFixture
    {
        private File File;
        private bool ShouldUseIndex;
        private bool IsFileInIndex;
        private bool IsFileInStorage;
        private SnapshotRecord SnapshotRecord;
        private readonly Hash Hash1 = new Hash(HashAlgorithmId.SHA256, new byte[] { 1, 2, 3 });
        private readonly Hash Hash2 = new Hash(HashAlgorithmId.SHA256, new byte[] { 4, 5, 6 });

        private Mock<StreamWriter> StreamWriterMock;
        private Mock<ISnapshotService> SnapshotServiceMock;
        private Mock<ICtlgService> CtlgServiceMock;
        private Mock<IIndexService> IndexServiceMock;
        private Mock<IFileStorageService> FileStorageService;

        private IList<BackupEntryCreated> BackupEntryCreatedEvents;

        [SetUp]
        public void SetUp()
        {
            File = new File("test");
            File.Hashes.Add(Hash1);
            ShouldUseIndex = false;
            IsFileInIndex = true;
            IsFileInStorage = true;

            SnapshotRecord = new SnapshotRecord(Hash2, DateTime.MinValue, 0, "");
            SnapshotServiceMock = AutoMock.Mock<ISnapshotService>();
            SnapshotServiceMock.Setup(s => s.CreateSnapshotRecord(File)).Returns(SnapshotRecord);

            CtlgServiceMock = AutoMock.Mock<ICtlgService>();
            CtlgServiceMock.Setup(s => s.CalculateHashForFile(File, It.IsAny<IHashFunction>()))
                .Callback<File, IHashFunction>(CalculateHashForFileImpl)
                .Returns(Hash2);

            IndexServiceMock = AutoMock.Mock<IIndexService>();
            IndexServiceMock.Setup(s => s.IsInIndex(Hash1.Value)).Returns(() => IsFileInIndex);

            FileStorageService = AutoMock.Mock<IFileStorageService>();
            FileStorageService.Setup(s => s.IsFileInStorage(File)).Returns(() => IsFileInStorage);

            var stream = new MemoryStream();
            StreamWriterMock = new Mock<StreamWriter>(stream);
            AutoMock.Provide(StreamWriterMock.Object);

            BackupEntryCreatedEvents = SetupEvents<BackupEntryCreated>();
        }

        [Test]
        public void AddFile_WhenHashIsKnown()
        {
            Execute_AddFile();

            FileStorageService.Verify(m => m.AddFileToStorage(File), Times.Never);
            Verify_BackupEventCreated(hashCalculated: false, isHashFoundInIndex: false, isFileAddedToStorage: false);
            Verify_StreamWriter();
        }

        [Test]
        public void AddFile_WhenHashIsNotKnown()
        {
            File.Hashes.Clear();

            Execute_AddFile();

            FileStorageService.Verify(m => m.AddFileToStorage(File), Times.Never);
            Verify_BackupEventCreated(hashCalculated: true, isHashFoundInIndex: false, isFileAddedToStorage: false);
        }

        [Test]
        public void AddFile_WhenUsesIndexAndHashIsInIndex()
        {
            ShouldUseIndex = true;

            Execute_AddFile();

            FileStorageService.Verify(m => m.AddFileToStorage(File), Times.Never);
            Verify_BackupEventCreated(hashCalculated: false, isHashFoundInIndex: true, isFileAddedToStorage: false);
        }

        [Test]
        public void AddFile_WhenUsesIndexAndHashIsNotInIndex()
        {
            ShouldUseIndex = true;
            IsFileInIndex = false;

            Execute_AddFile();

            FileStorageService.Verify(m => m.AddFileToStorage(File), Times.Never);
            Verify_BackupEventCreated(hashCalculated: false, isHashFoundInIndex: false, isFileAddedToStorage: false);
        }

        [Test]
        public void AddFile_WhenFileIsNotInStorage()
        {
            File.Hashes.Clear();
            IsFileInStorage = false;

            Execute_AddFile();

            Assert.That(File.Hashes.First(), Is.EqualTo(Hash2));
            FileStorageService.Verify(m => m.AddFileToStorage(File), Times.Once);
            IndexServiceMock.Verify(s => s.Add(Hash2.Value), Times.Once);
            Verify_BackupEventCreated(hashCalculated: true, isHashFoundInIndex: false, isFileAddedToStorage: true);
        }

        [Test]
        public void AddFile_WhenHashIsKnownButFileIsNotInStorage()
        {
            IsFileInStorage = false;

            Execute_AddFile();

            Assert.That(File.Hashes.First(), Is.EqualTo(Hash2));
            FileStorageService.Verify(m => m.AddFileToStorage(File), Times.Once);
            Verify_BackupEventCreated(hashCalculated: true, isHashFoundInIndex: false, isFileAddedToStorage: true);
        }

        private void CalculateHashForFileImpl(File f, IHashFunction hashFunction)
        {
            f.Hashes.Add(Hash2);
        }

        private void Execute_AddFile()
        {
            var writer = AutoMock.Create<BackupWriter>(new NamedParameter("shouldUseIndex", ShouldUseIndex));

            writer.AddFile(File);
        }

        private void Verify_BackupEventCreated(bool hashCalculated, bool isHashFoundInIndex, bool isFileAddedToStorage)
        {
            var created = BackupEntryCreatedEvents.First();

            Assert.That(created.HashCalculated, Is.EqualTo(hashCalculated));
            Assert.That(created.IsHashFoundInIndex, Is.EqualTo(isHashFoundInIndex));
            Assert.That(created.NewFileAddedToStorage, Is.EqualTo(isFileAddedToStorage));
        }

        private void Verify_StreamWriter()
        {
            StreamWriterMock.Verify(m => m.WriteLine(SnapshotRecord), Times.Once);
        }
    }
}
