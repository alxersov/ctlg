﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
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

        public static void SetupGetBackupFilePath(this AutoMock mock, string hash, string path)
        {
            mock.Mock<IFileStorageService>()
                .Setup(s => s.GetBackupFilePath(It.Is<string>(h => h == hash), null))
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
            mock.Mock<ISnapshotService>().Setup(s => s.FindSnapshotPath(backupName, null)).Returns(snapshotPath);
        }

        public static void SetupReadSnapshotFile(this AutoMock mock, string path, IEnumerable<SnapshotRecord> snapshotRecords)
        {
            mock.Mock<ISnapshotService>().Setup(s => s.ReadSnapshotFile(It.Is<string>(p => p == path))).Returns(snapshotRecords);
        }

        public static Mock<StreamWriter> SetupCreateSnapshotWriter(this AutoMock mock, string name, string date)
        {
            var stream = new MemoryStream();
            var streamWriterMock = new Mock<StreamWriter>(stream);
            mock.Mock<ISnapshotService>().Setup(s => s.CreateSnapshotWriter(name, date))
                .Returns(streamWriterMock.Object);

            return streamWriterMock;
        }

        public static Mock<IBackupWriter> SetupBackupWriter(this AutoMock mock, string name, string timestamp,
            Expression<Func<bool, bool>> shouldUseIndex, bool shouldExistingHashMatch)
        {
            var backupWriterMock = new Mock<IBackupWriter>();

            mock.Mock<ICtlgService>()
                .Setup(p => p.CreateBackupWriter(
                    name, timestamp, It.Is(shouldUseIndex), shouldExistingHashMatch))
                .Returns(backupWriterMock.Object);

            return backupWriterMock;
        }

        public static void VerifyAppVersionWritten(this Mock<IBackupWriter> backupWriterMock)
        {
            backupWriterMock.Verify(m => m.AddComment(It.Is<string>(
                s => SnapshotCommentRegEx.IsMatch(s))));
        }

        private static readonly Regex SnapshotCommentRegEx = new Regex(@"^ctlg \d*\.\d*\.\d*\.\d*$");
    }
}
