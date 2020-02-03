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

        public BackupCommand(ITreeProvider treeProvider, ISnapshotReader snapshotReader,
            IFileStorageService fileStorageService,
            IFilesystemService filesystemService, ISnapshotService snapshotService)
        {
            TreeProvider = treeProvider;
            SnapshotReader = snapshotReader;
            FileStorageService = fileStorageService;
            FilesystemService = filesystemService;
            SnapshotService = snapshotService;
        }

        public void Execute()
        {
            var currentDirectory = FilesystemService.GetCurrentDirectory();

            var root = TreeProvider.ReadTree(Path, SearchPattern);

            if (IsFastMode)
            {
                var latestSnapshot = SnapshotService.GetSnapshot(currentDirectory, Name, null);
                if (latestSnapshot != null)
                {
                    SnapshotReader.ReadHashesFromSnapshot(latestSnapshot, root);
                }
            }

            using (var fileStorage = FileStorageService.GetFileStorage(currentDirectory, IsFastMode, false))
            {
                var snapshot = SnapshotService.CreateSnapshot(currentDirectory, Name, null);

                using (var snapshotWriter = snapshot.GetWriter())
                {
                    snapshotWriter.AddComment($"ctlg {AppVersion.Version}");
                    snapshotWriter.AddComment($"FastMode={IsFastMode}");
                    var backupWriter = new BackupWriter(fileStorage, snapshotWriter);
                    var treeWalker = new TreeWalker(root);
                    treeWalker.Walk(backupWriter.AddFile);
                }
            }

            DomainEvents.Raise(new BackupCommandEnded());
        }

        private ITreeProvider TreeProvider { get; set; }
        private IFileStorageService FileStorageService { get; }
        private IFilesystemService FilesystemService { get; }
        public ISnapshotService SnapshotService { get; }
        private ISnapshotReader SnapshotReader { get; set; }
    }
}
