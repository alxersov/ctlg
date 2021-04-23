using System;
using System.Linq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Commands.Steps;
using Ctlg.Service.Events;

namespace Ctlg.Service.Commands
{
    public class BackupCommand: ICommand
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string SearchPattern { get; set; }
        public bool IsFastMode { get; set; }

        public BackupCommand(IFilesystemService filesystemService,
            ISnapshotService snapshotService, IBackupService backupService)
        {
            FilesystemService = filesystemService;
            SnapshotService = snapshotService;
            BackupService = backupService;
        }

        public void Execute(Config config)
        {
            var search = new FileSearch(FilesystemService, Path, SearchPattern);
            var files = search.Run().ToList();

            ISnapshot latestSnapshot = IsFastMode ? SnapshotService.FindSnapshot(config, Name, null) : null;
               
            using (var backupWriter = BackupService.CreateWriter(config, Name, null, IsFastMode))
            {
                backupWriter.AddComment($"FastMode={IsFastMode}");
                foreach (var file in files)
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

        public IFilesystemService FilesystemService { get; }
        private ISnapshotService SnapshotService { get; }
        private IBackupService BackupService { get; }
    }
}
