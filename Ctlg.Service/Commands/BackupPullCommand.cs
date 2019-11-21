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

        public BackupPullCommand(ISnapshotService snapshotService, ICtlgService ctlgService,
            IFileStorageService fileStorageService, IIndexFileService indexFileService)
        {
            SnapshotService = snapshotService;
            CtlgService = ctlgService;
            FileStorageService = fileStorageService;
            IndexFileService = indexFileService;
        }

        public void Execute()
        {
            IndexFileService.Load();

            var snapshotFile = SnapshotService.FindSnapshotFile(Path, Name, Date);
            using (var backupWriter = CtlgService.CreateBackupWriter(snapshotFile.Name, snapshotFile.Date, false, true))
            {
                backupWriter.AddComment($"ctlg {AppVersion.Version}");
                backupWriter.AddComment($"Created with pull-backup command.");

                foreach (var snapshotRecord in SnapshotService.ReadSnapshotFile(snapshotFile.FullPath))
                {
                    var file = SnapshotService.CreateFile(snapshotRecord);
                    file.FullPath = FileStorageService.GetBackupFilePath(snapshotRecord.Hash, Path);

                    backupWriter.AddFile(file);
                }
            }

            IndexFileService.Save();

            DomainEvents.Raise(new BackupCommandEnded());
        }

        private ISnapshotService SnapshotService { get; }
        private ICtlgService CtlgService { get; }
        private IFileStorageService FileStorageService { get; }
        private IIndexFileService IndexFileService { get; }
    }
}
