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
            IBackupService backupService, IConfigService configService)
        {
            SnapshotService = snapshotService;
            FileStorageService = fileStorageService;
            BackupService = backupService;
            ConfigService = configService;
        }

        public void Execute(Config config)
        {
            var srouceConfig = ConfigService.LoadConfig(Path);
            var sourceSnapshot = SnapshotService.GetSnapshot(Path, srouceConfig.HashAlgorithmName, Name, Date);
            if (sourceSnapshot == null)
            {
                throw new Exception($"Snapshot {Name} is not found in {Path}.");
            }

            var sourceFileStorage = FileStorageService.GetFileStorage(Path, srouceConfig.HashAlgorithmName);
            using (var backupWriter = BackupService.CreateWriter(config.Path, false, config.HashAlgorithmName,
                sourceSnapshot.Name, sourceSnapshot.Timestamp))
            {
                backupWriter.AddComment($"ctlg {AppVersion.Version}");
                backupWriter.AddComment($"Created with pull-backup command.");

                foreach (var file in sourceSnapshot.EnumerateFiles())
                {
                    backupWriter.AddFile(file, sourceFileStorage);
                }
            }

            DomainEvents.Raise(new BackupCommandEnded());
        }

        private ISnapshotService SnapshotService { get; }
        private IFileStorageService FileStorageService { get; }
        private IBackupService BackupService { get; }
        public IConfigService ConfigService { get; }
    }
}
