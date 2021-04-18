using System;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;

namespace Ctlg.Service.Commands
{
    public class BackupCommand: ICommand
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string SearchPattern { get; set; }
        public bool IsFastMode { get; set; }

        public BackupCommand(ITreeProvider treeProvider,
            ISnapshotService snapshotService, IBackupService backupService)
        {
            TreeProvider = treeProvider;
            SnapshotService = snapshotService;
            BackupService = backupService;
        }

        public void Execute(Config config)
        {
            var root = TreeProvider.ReadTree(Path, SearchPattern);

            ISnapshot latestSnapshot = IsFastMode ? SnapshotService.FindSnapshot(config, Name, null) : null;
               
            using (var backupWriter = BackupService.CreateWriter(config, Name, null, IsFastMode))
            {
                backupWriter.AddComment($"FastMode={IsFastMode}");
                foreach (var file in root.EnumerateFiles())
                {
                    backupWriter.AddFile(file, Path, GetPreviousHashForFile(latestSnapshot, file));
                }
            }

            DomainEvents.Raise(new BackupCommandEnded());
        }

        private byte[] GetPreviousHashForFile(ISnapshot latestSnapshot, File file)
        {
            byte[] hash = null;
            var record = latestSnapshot?.GetRecord(file.RelativePath);
            if (record != null && record.Size == file.Size && record.FileModifiedDateTime == file.FileModifiedDateTime)
            {
                hash = record.Hash;
            }
            return hash;
        }

        private ITreeProvider TreeProvider { get; set; }
        private ISnapshotService SnapshotService { get; }
        private IBackupService BackupService { get; }
    }
}
