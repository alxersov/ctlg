using System;
using System.IO;
using System.Linq;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Services;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Services
{
    [TestFixture]
    public class FileIndexTests
    {
        private byte[] Hash1 = { 1,1 };
        private byte[] Hash2 = { 2,2 };
        private byte[] Hash3 = { 3,3 };
        private byte[] HashNotInIndex = { 4,4 };
        private byte[] HashOtherLength = { 1 };
        private string Path = "foo";
        private int HashLength = 2;

        private Mock<IFilesystemService> FilesystemServiceMock;
        private FileIndex FileIndex;

        [SetUp]
        public void Init()
        {
            FilesystemServiceMock = new Mock<IFilesystemService>();
            FileIndex = CreateFileIndex();
            FileIndex.Add(Hash1);
            FileIndex.Add(Hash2);
        }

        [Test]
        public void IsInIndex()
        {
            Assert.That(FileIndex.IsInIndex(HashNotInIndex), Is.False);
            FileIndex.Add(HashNotInIndex);
            Assert.That(FileIndex.IsInIndex(HashNotInIndex), Is.True);
        }

        [Test]
        public void GetAllHashes_Returns_PreviouslyAddedHashes()
        {
            Assert.That(FileIndex.GetAllHashes(), Is.EquivalentTo(new[] { Hash1, Hash2 }));
        }

        [Test]
        public void Add_WhenHashHasUnexpectedLength_ThrowsException()
        {
            Assert.That(() => FileIndex.Add(HashOtherLength),
                Throws.InstanceOf<Exception>()
                    .With.Message.Contain("Expected hash to have length 2 bytes"));
        }

        [Test]
        public void Save_writes_to_file()
        {
            var stream = new MemoryStream();
            FilesystemServiceMock.Setup(s => s.CreateFileForWrite(Path)).Returns(stream);
            FileIndex.Save();
            Assert.That(stream.ToArray(), Is.EquivalentTo(Hash1.Concat(Hash2)));
        }

        [Test]
        public void Load()
        {
            FileIndex = CreateFileIndex();
            FilesystemServiceMock.SetupOpenFileForRead(Path, Hash1.Concat(Hash3).ToArray());
            FileIndex.Load();
            Assert.That(FileIndex.GetAllHashes(), Is.EquivalentTo(new[] { Hash1, Hash3 }));
        }

        [Test]
        public void Load_when_corrupted_file()
        {
            FilesystemServiceMock.SetupOpenFileForRead(Path, Hash1.Concat(HashOtherLength).ToArray());
            Assert.That(() => FileIndex.Load(),
               Throws.InstanceOf<Exception>()
                   .With.Message.Contain("Corrupted index file"));
        }

        [Test]
        public void Load_when_file_does_not_exist()
        {
            FilesystemServiceMock.Setup(s => s.OpenFileForRead(Path))
                .Throws(new FileNotFoundException());
            FileIndex.Load();
            FilesystemServiceMock.Verify(s => s.OpenFileForRead(Path), Times.Once);
        }

        private FileIndex CreateFileIndex()
        {
            return new FileIndex(FilesystemServiceMock.Object, Path, HashLength);
        }
    }
}
