using System;
using Autofac.Extras.Moq;
using Ctlg.Service.Commands;
using Ctlg.Service.Events;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    public class RestoreCommandTests: BackupTestFixture
    {
        [Test]
        public void Execute_WhenBadFileList_RaisesExceptionEvent()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.SetupOpenFileForRead(BackupName, "Bad line");

                var events = SetupEvents<ExceptionEvent>();

                Execute(mock);

                Assert.That(events.Count, Is.EqualTo(1));
                Assert.That(events[0].Exception.Message, Does.Contain("Unexpected list line"));
            }
        }

        [Test]
        public void Execute_WhenBackupFileNotFound_RaisesExceptionEvent()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.SetupOpenFileForRead(BackupName, FileListLine);

                mock.SetupGetBackupFilePath(Hash, BackupFileName);

                mock.SetupFileExists(BackupFileName, false);

                var events = SetupEvents<ExceptionEvent>();

                Execute(mock);

                Assert.That(events.Count, Is.EqualTo(1));
                Assert.That(events[0].Exception.Message, Does.Contain($"{BackupFileName} not found"));
            }
        }

        [Test]
        public void Execute_WhenRestoreIsSuccessfull_RaisesBackupProcessedEvent()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.SetupOpenFileForRead(BackupName, FileListLine);

                mock.SetupGetBackupFilePath(Hash, BackupFileName);

                mock.SetupFileExists(BackupFileName, true);

                var events = SetupEvents<BackupEntryRestored>();

                Execute(mock);

                Assert.That(events.Count, Is.EqualTo(1));
                Assert.That(events[0].BackupEntry, Does.Contain(FilePath));
            }
        }

        private void Execute(AutoMock mock)
        {
            var command = mock.Create<RestoreCommand>();
            command.Name = BackupName;
            command.Path = @"C:\foo";

            command.Execute(null);
        }
    }
}
