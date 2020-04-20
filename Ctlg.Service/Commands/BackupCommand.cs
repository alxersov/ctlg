using System;
using Ctlg.Core.Interfaces;
using Ctlg.Core.Utils;
using Ctlg.Service.Events;

namespace Ctlg.Service.Commands
{
    public class BackupCommand: ICommand
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string SearchPattern { get; set; }
        public bool IsFastMode { get; set; }

        public BackupCommand(ITreeProvider treeProvider, IFilesystemService filesystemService,
            ISnapshotService snapshotService, IBackupService backupService)
        {
            TreeProvider = treeProvider;
            FilesystemService = filesystemService;
            SnapshotService = snapshotService;
            BackupService = backupService;
        }

        public void Execute()
        {
            var currentDirectory = FilesystemService.GetCurrentDirectory();

            var root = TreeProvider.ReadTree(Path, SearchPattern);

            if (IsFastMode)
            {
                var latestSnapshot = SnapshotService.GetSnapshot(currentDirectory, "SHA-256", Name, null);
                if (latestSnapshot != null)
                {
                    var reader = new SnapshotReader();
                    reader.ReadHashesFromSnapshot(latestSnapshot, root);
                }
            }

            using (var backupWriter = BackupService.CreateWriter(currentDirectory, IsFastMode, "SHA-256", Name, null))
            {
                backupWriter.AddComment($"ctlg {AppVersion.Version}");
                backupWriter.AddComment($"FastMode={IsFastMode}");
                foreach (var file in root.EnumerateFiles())
                {
                    backupWriter.AddFile(file);
                }
            }

            DomainEvents.Raise(new BackupCommandEnded());
        }

        private ITreeProvider TreeProvider { get; set; }
        private IFilesystemService FilesystemService { get; }
        private ISnapshotService SnapshotService { get; }
        private IBackupService BackupService { get; }
    }
}
