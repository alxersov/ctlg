using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Autofac.Extras.Moq;
using Autofac.Features.Indexed;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Moq;
using File = Ctlg.Core.File;

namespace Ctlg.UnitTests
{
    public static class MockHelper
    {
        public static void SetupFileExists(this AutoMock mock, string path, bool exists)
        {
            mock.Mock<IFilesystemService>()
                .Setup(s => s.FileExists(It.Is<string>(p => p == path)))
                .Returns(exists);
        }

        public static IFilesystemDirectory MockDirectory(string name,
            IEnumerable<File> childFiles,
            IEnumerable<IFilesystemDirectory> childDirectories = null)
        {
            var fsDirectory = new Mock<IFilesystemDirectory>();

            fsDirectory.Setup(d => d.EnumerateDirectories()).Returns(childDirectories);

            fsDirectory.Setup(d => d.EnumerateFiles(It.IsAny<string>())).Returns(childFiles);

            fsDirectory.SetupGet(d => d.Directory).Returns(new File(name, true));

            return fsDirectory.Object;
        }

        public static void SetupOpenFileForRead(this Mock<IFilesystemService> mock, string path, byte[] content)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            mock.Setup(s => s.OpenFileForRead(path)).Returns(stream);
        }

        public static void SetupOpenFileForRead(this AutoMock mock, string path, string content)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            mock.Mock<IFilesystemService>().Setup(s => s.OpenFileForRead(path)).Returns(stream);
        }

        public static MemoryStream SetupCreateNewFileForWrite(this AutoMock mock)
        {
            var fileSystemServiceMock = mock.Mock<IFilesystemService>();
            var stream = new MemoryStream();
            fileSystemServiceMock
                .Setup(s => s.CreateNewFileForWrite(It.IsAny<string>()))
                .Returns(stream);
            return stream;
        }

        public static void SetupHashFunction(this AutoMock mock, string hashFunctionName, Hash valueToReturn)
        {
            var hashFunctionMock = new Mock<IHashFunction>();
            hashFunctionMock.Setup(f => f.CalculateHash(It.IsAny<Stream>()))
                            .Returns(valueToReturn);

            var index = new Index<string, IHashFunction>();
            index.Add(hashFunctionName, hashFunctionMock.Object);
            mock.Provide<IIndex<string, IHashFunction>>(index);
        }

        public static void VerifyAppVersionWritten(this Mock<ISnapshotWriter> backupWriterMock)
        {
            backupWriterMock.Verify(m => m.AddComment(It.Is<string>(
                s => AppVersionRegEx.IsMatch(s))));
        }

        public static void SetupPath(this Mock<IFilesystemService> mock,
            string path1, string path2, string result)
        {
            mock.Setup(m => m.CombinePath(path1, path2)).Returns(result);
            mock.Setup(m => m.GetDirectoryName(result)).Returns(path1);
        }

        public static void SetupPath(this Mock<IFilesystemService> mock,
            string path1, string path2, string path3, string result)
        {
            mock.Setup(m => m.CombinePath(path1, path2, path3)).Returns(result);
        }

        public static void VerifyCopyNeverCalled(this Mock<IFilesystemService> mock)
        {
            mock.Verify(m => m.Copy(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        public static Mock<IFileStorage> SetupGetFileStorage(this Mock<IFileStorageService> mock,
            string backupRootDirectory, bool shouldUseIndex, bool shouldExistingHashMatchCaclulated)
        {
            var fileStorageMock = new Mock<IFileStorage>();

            mock.Setup(s => s.GetFileStorage(backupRootDirectory, shouldUseIndex, shouldExistingHashMatchCaclulated))
                .Returns(fileStorageMock.Object);

            return fileStorageMock;
        }

        public static Mock<ISnapshotWriter> SetupCreateSnapshot(this Mock<ISnapshotService> mock,
            string backupRootPath, string name, string timestamp)
        {
            var snapshotMock = new Mock<ISnapshot>();
            mock.Setup(s => s.CreateSnapshot(backupRootPath, name, timestamp))
                .Returns(snapshotMock.Object);

            var snapshotWriterMock = new Mock<ISnapshotWriter>();
            snapshotMock.Setup(s => s.GetWriter()).Returns(snapshotWriterMock.Object);
            return snapshotWriterMock;
        }

        private static readonly Regex AppVersionRegEx = new Regex(@"^ctlg \d*\.\d*\.\d*\.\d*$");
    }
}
