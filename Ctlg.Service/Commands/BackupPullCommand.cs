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

        public BackupPullCommand(ISnapshotService snapshotService,
            IFileStorageService fileStorageService, IFilesystemService filesystemService)
        {
            SnapshotService = snapshotService;
            FileStorageService = fileStorageService;
            FilesystemService = filesystemService;
        }

        public void Execute()
        {
            var sourceSnapshot = SnapshotService.GetSnapshot(Path, Name, Date);
            if (sourceSnapshot == null)
            {
                throw new Exception($"Snapshot {Name} is not found in {Path}.");
            }

            var currentDirectory = FilesystemService.GetCurrentDirectory();
            using (var fileStorage = FileStorageService.GetFileStorage(currentDirectory, false, true))
            {
                var sourceFileStorage = FileStorageService.GetFileStorage(Path, true, true);
                var destinationSnapshot = SnapshotService.CreateSnapshot(currentDirectory,
                    sourceSnapshot.Name, sourceSnapshot.Timestamp);
                using (var snapshotWriter = destinationSnapshot.GetWriter())
                {
                    snapshotWriter.AddComment($"ctlg {AppVersion.Version}");
                    snapshotWriter.AddComment($"Created with pull-backup command.");

                    var backupWriter = new BackupWriter(fileStorage, snapshotWriter);

                    foreach (var snapshotRecord in sourceSnapshot.EnumerateFiles())
                    {
                        var file = SnapshotService.CreateFile(snapshotRecord);
                        file.FullPath = sourceFileStorage.GetBackupFilePath(snapshotRecord.Hash);

                        backupWriter.AddFile(file);
                    }
                }
            }

            DomainEvents.Raise(new BackupCommandEnded());
        }

        private ISnapshotService SnapshotService { get; }
        private IFileStorageService FileStorageService { get; }
        private IFilesystemService FilesystemService { get; }
    }
}
