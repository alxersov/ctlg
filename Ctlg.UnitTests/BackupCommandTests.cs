using System;
using System.IO;
using Autofac.Extras.Moq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Commands;
using Ctlg.Service.Utils;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    public class BackupCommandTests: BackupTestFixture
    {
        [Test]
        public void Execute_CreatesFileListEntry()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.SetupHashFunction("SHA-256", new Hash(HashAlgorithmId.SHA256, FormatBytes.ToByteArray(Hash)));
                mock.SetupGetDirectory(SourcePath);
                var stream = mock.SetupCreateNewFileForWrite();

                Execute(mock);

                var reader = new StreamReader(new MemoryStream(stream.ToArray()));
                string fileListLine = reader.ReadToEnd().Trim();

                Assert.That(fileListLine, Is.EqualTo(FileListLine));
            }
        }

        [Test]
        public void Execute_WhenFileWasNotProcessedBefore_CopiesItToStorageFolder()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.SetupHashFunction("SHA-256", new Hash(HashAlgorithmId.SHA256, FormatBytes.ToByteArray(Hash)));
                mock.SetupGetDirectory(SourcePath);
                mock.SetupCreateNewFileForWrite();
                mock.SetupGetBackupFilePath(Hash, BackupFileName);
                mock.SetupGetDirectoryName(BackupFileName, BackupDirectory);
                mock.SetupFileExists(BackupFileName, false);

                Execute(mock);

                mock.Mock<IFilesystemService>().Verify(s => s.CreateDirectory(BackupDirectory), Times.Once);
                mock.Mock<IFilesystemService>().Verify(s => s.Copy(SourceFilePath, BackupFileName), Times.Once);
            }
        }

        private void Execute(AutoMock mock)
        {
            var command = mock.Create<BackupCommand>();
            command.Name = BackupName;
            command.Path = SourcePath;

            command.Execute(null);
        }
    }
}
