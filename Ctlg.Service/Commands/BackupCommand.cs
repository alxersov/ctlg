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

        public BackupCommand(ITreeProvider treeProvider, ISnapshotReader snapshotReader, ICtlgService ctlgService,
            IIndexFileService indexFileService)
        {
            TreeProvider = treeProvider;
            SnapshotReader = snapshotReader;
            CtlgService = ctlgService;
            IndexFileService = indexFileService;
        }

        public void Execute()
        {
            IndexFileService.Load();

            var root = TreeProvider.ReadTree(Path, SearchPattern);

            if (IsFastMode)
            {
                SnapshotReader.ReadHashesFromLatestSnapshot(Name, root);
            }

            var treeWalker = new TreeWalker(root);

            using (var writer = CtlgService.CreateBackupWriter(Name, null, IsFastMode, false))
            {
                writer.AddComment($"ctlg {AppVersion.Version}");
                writer.AddComment($"FastMode={IsFastMode}");
                treeWalker.Walk(writer.AddFile);
            }

            IndexFileService.Save();

            DomainEvents.Raise(new BackupCommandEnded());
        }

        private ITreeProvider TreeProvider { get; set; }
        private ICtlgService CtlgService { get; set; }
        private IIndexFileService IndexFileService { get; set; }
        private ISnapshotReader SnapshotReader { get; set; }
    }
}
