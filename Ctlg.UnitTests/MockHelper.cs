using System;
using System.IO;
using System.Text.RegularExpressions;
using Autofac.Extras.Moq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Moq;

namespace Ctlg.UnitTests
{
    public static class MockHelper
    {

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

        public static void VerifyAppVersionWritten(this Mock<IBackupWriter> backupWriterMock)
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

        public static Mock<IFileStorage> SetupGetFileStorage(this Mock<IFileStorageService> mock,
            string backupRootDirectory)
        {
            var fileStorageMock = new Mock<IFileStorage>();

            mock.Setup(s => s.GetFileStorage(backupRootDirectory, "SHA-256"))
                .Returns(fileStorageMock.Object);

            return fileStorageMock;
        }

        public static Mock<IBackupWriter> SetupCreateWriter(this Mock<IBackupService> mock,
            string backupRootPath, string name, string timestamp)
        {
            var backupWriterMock = new Mock<IBackupWriter>();
            mock.Setup(s => s.CreateWriter(backupRootPath, It.IsAny<bool>(), "SHA-256", name, timestamp))
                .Returns(backupWriterMock.Object);
            return backupWriterMock;
        }

        public static void SetupHashAlgorithm(this AutoMock mock, HashAlgorithm hashAlgorithm)
        {
            mock.Mock<IDataService>()
                .Setup(s => s.GetHashAlgorithm(It.Is<string>(p => p == hashAlgorithm.Name)))
                .Returns(hashAlgorithm);
        }

        private static readonly Regex AppVersionRegEx = new Regex(@"^ctlg \d*\.\d*\.\d*\.\d*$");
    }
}
