using System;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Commands
{
    public sealed class BackupPullCommand: ICommand
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }

        public BackupPullCommand(ISnapshotService snapshotService, ICtlgService ctlgService,
            IFileStorageService fileStorageService)
        {
            SnapshotService = snapshotService;
            CtlgService = ctlgService;
            FileStorageService = fileStorageService;
        }

        public void Execute()
        {
            var snapshotFile = SnapshotService.FindSnapshotFile(Path, Name, Date);
            using (var backupWriter = CtlgService.CreateBackupWriter(snapshotFile.Name, snapshotFile.Date, false, true))
            {
                foreach (var snapshotRecord in SnapshotService.ReadSnapshotFile(snapshotFile.FullPath))
                {
                    var file = SnapshotService.CreateFile(snapshotRecord);
                    file.FullPath = FileStorageService.GetBackupFilePath(snapshotRecord.Hash, Path);

                    backupWriter.AddFile(file);
                }
            }
        }

        private ISnapshotService SnapshotService { get; }
        private ICtlgService CtlgService { get; }
        private IFileStorageService FileStorageService { get; }
    }
}
