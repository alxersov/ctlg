using System;
using System.Collections.Generic;
using System.IO;
using Autofac.Extras.Moq;
using Autofac.Features.Indexed;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service;
using Moq;

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

        public static void SetupGetDirectory(this AutoMock mock, string path)
        {
            var fsDirectory = mock.Mock<IFilesystemDirectory>();

            fsDirectory.Setup(d => d.EnumerateDirectories())
                       .Returns(new IFilesystemDirectory[0]);

            var file = new Core.File("1.txt");
            file.FullPath = @"C:\foo\1.txt";
            file.RelativePath = file.Name;
            file.Size = 11;
            file.FileModifiedDateTime = new DateTime(2018, 4, 22, 18, 5, 12, 0, DateTimeKind.Utc);
            fsDirectory.Setup(d => d.EnumerateFiles(It.IsAny<string>()))
                       .Returns(new Core.File[] { file });

            var rootDir = new Ctlg.Core.File(path, true);
            rootDir.FullPath = path;

            fsDirectory.SetupGet(d => d.Directory)
                       .Returns(rootDir);

            mock.Mock<IFilesystemService>()
                .Setup(s => s.GetDirectory(It.Is<string>(p => p == path)))
                .Returns(fsDirectory.Object);
        }

        public static void SetupGetDirectoryName(this AutoMock mock, string filePath, string directoryPath)
        {
            mock.Mock<IFilesystemService>()
                .Setup(s => s.GetDirectoryName(It.Is<string>(p => p == filePath)))
                .Returns(directoryPath);
        }

        public static void SetupGetBackupFilePath(this AutoMock mock, string hash, string path)
        {
            mock.Mock<ICtlgService>()
                .Setup(s => s.GetBackupFilePath(It.Is<string>(h => h == hash)))
                .Returns(path);
        }

        public static void SetupOpenFileForRead(this AutoMock mock, string fileName, string content)
        {
            var fileSystemServiceMock = mock.Mock<IFilesystemService>();

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            fileSystemServiceMock
                .Setup(s => s.OpenFileForRead(It.Is<string>(name => name == fileName)))
                .Returns(stream);
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

        public static void SetupFindSnapshotFile(this AutoMock mock, string backupName, string snapshotPath)
        {
            mock.Mock<ISnapshotService>().Setup(s => s.FindSnapshotFile(It.Is<string>(name => name == backupName), It.Is<string>(date => date == null))).Returns(snapshotPath);
        }

        public static void SetupReadSnapshotFile(this AutoMock mock, string path, IEnumerable<SnapshotRecord> snapshotRecords)
        {
            mock.Mock<ISnapshotService>().Setup(s => s.ReadSnapshotFile(It.Is<string>(p => p == path))).Returns(snapshotRecords);
        }
    }
}
