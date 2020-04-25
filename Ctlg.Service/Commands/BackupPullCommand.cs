using System;
using Ctlg.Core;
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
            IBackupService backupService)
        {
            SnapshotService = snapshotService;
            FileStorageService = fileStorageService;
            BackupService = backupService;
        }

        public void Execute(Config config)
        {
            var sourceSnapshot = SnapshotService.GetSnapshot(Path, config.HashAlgorithmName, Name, Date);
            if (sourceSnapshot == null)
            {
                throw new Exception($"Snapshot {Name} is not found in {Path}.");
            }

            var sourceFileStorage = FileStorageService.GetFileStorage(Path, config.HashAlgorithmName);
            using (var backupWriter = BackupService.CreateWriter(config.Path, false, config.HashAlgorithmName,
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
        private IBackupService BackupService { get; }
    }
}
