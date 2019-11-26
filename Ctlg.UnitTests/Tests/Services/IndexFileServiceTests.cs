using System;
using System.IO;
using Autofac;
using Autofac.Extras.Moq;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Services;
using Ctlg.UnitTests.Fixtures;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Services
{
    public class IndexFileServiceTests : AutoMockTestFixture
    {
        private Mock<IIndexService> IndexServiceMock;
        private Mock<IFilesystemService> FilesystemServiceMock;
        private IndexFileService IndexFileService;

        private const string IndexFilePath = "current-dir/index.bin";
        private readonly byte[] Hash1 = { 1, 2 };
        private readonly byte[] Hash2 = { 3, 4 };
        private readonly byte[] AllHashes = { 1, 2, 3, 4 };
        private readonly byte[] BadIndex = { 1, 2, 3 };

        [SetUp]
        public void Setup()
        {
            IndexServiceMock = AutoMock.Mock<IIndexService>();
            FilesystemServiceMock = AutoMock.Mock<IFilesystemService>();

            FilesystemServiceMock.Setup(s => s.GetCurrentDirectory()).Returns("current-dir");
            FilesystemServiceMock
                .Setup(s => s.CombinePath("current-dir", "index.bin"))
                .Returns(IndexFilePath);

            IndexFileService = AutoMock.Create<IndexFileService>(new NamedParameter("hashLength", 2));
        }


        [Test]
        public void Save_WhenCalled_WritesIndexToFile()
        {
            IndexServiceMock
                .Setup(s => s.GetAllHashes())
                .Returns(new[] { Hash1, Hash2 });


            var stream = new MemoryStream();
            FilesystemServiceMock
                .Setup(s => s.CreateFileForWrite(IndexFilePath))
                .Returns(stream);

            IndexFileService.Save();

            Assert.That(stream.ToArray(), Is.EqualTo(AllHashes));
        }

        [Test]
        public void Load_WhenCalled_AddsHashesToIndex()
        {
            var stream = new MemoryStream(AllHashes);
            FilesystemServiceMock
                .Setup(s => s.OpenFileForRead(IndexFilePath))
                .Returns(stream);

            IndexFileService.Load();

            IndexServiceMock.Verify(s => s.Add(Hash1), Times.Once);
            IndexServiceMock.Verify(s => s.Add(Hash2), Times.Once);
        }

        [Test]
        public void Load_WhenIndexFileLengthIsNotCorrect_ThrowsException()
        {
            var stream = new MemoryStream(BadIndex);
            FilesystemServiceMock
                .Setup(s => s.OpenFileForRead(IndexFilePath))
                .Returns(stream);

            Assert.That(IndexFileService.Load,
                Throws.InstanceOf<Exception>()
                    .With.Message.Contain("Corrupted index file"));
        }
    }
}
