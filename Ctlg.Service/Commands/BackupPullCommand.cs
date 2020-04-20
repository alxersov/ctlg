using System;
using Ctlg.Core.Interfaces;
using Ctlg.Core.Utils;
using Ctlg.Service.Events;

namespace Ctlg.Service.Commands
{
    public sealed class BackupPullCommand: ICommand
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }

        public BackupPullCommand(ISnapshotService snapshotService, IFileStorageService fileStorageService,
            IFilesystemService filesystemService, IBackupService backupService)
        {
            SnapshotService = snapshotService;
            FileStorageService = fileStorageService;
            FilesystemService = filesystemService;
            BackupService = backupService;
        }

        public void Execute()
        {
            var sourceSnapshot = SnapshotService.GetSnapshot(Path, "SHA-256", Name, Date);
            if (sourceSnapshot == null)
            {
                throw new Exception($"Snapshot {Name} is not found in {Path}.");
            }

            var currentDirectory = FilesystemService.GetCurrentDirectory();
            var sourceFileStorage = FileStorageService.GetFileStorage(Path, "SHA-256");
            using (var backupWriter = BackupService.CreateWriter(currentDirectory, false, "SHA-256",
                sourceSnapshot.Name, sourceSnapshot.Timestamp))
            {
                backupWriter.AddComment($"ctlg {AppVersion.Version}");
                backupWriter.AddComment($"Created with pull-backup command.");

                foreach (var snapshotRecord in sourceSnapshot.EnumerateFiles())
                {
                    var file = SnapshotService.CreateFile(snapshotRecord);
                    backupWriter.AddFile(file, sourceFileStorage);
                }
            }

            DomainEvents.Raise(new BackupCommandEnded());
        }

        private ISnapshotService SnapshotService { get; }
        private IFileStorageService FileStorageService { get; }
        private IFilesystemService FilesystemService { get; }
        private IBackupService BackupService { get; }
    }
}
