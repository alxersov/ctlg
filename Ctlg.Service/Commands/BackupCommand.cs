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

        public BackupCommand(ITreeProvider treeProvider, ISnapshotService snapshotService, ISnapshotReader snapshotReader)
        {
            TreeProvider = treeProvider;
            SnapshotService = snapshotService;
            SnapshotReader = snapshotReader;
        }

        public void Execute()
        {
            var root = TreeProvider.ReadTree(Path, SearchPattern);

            if (IsFastMode)
            {
                SnapshotReader.ReadHashesFromLatestSnapshot(Name, root);
            }

            var treeWalker = new TreeWalker(root);

            using (var snapshot = SnapshotService.CreateSnapshotWriter(Name))
            {
                treeWalker.Walk(snapshot.AddFile);
            }

            DomainEvents.Raise(new BackupCommandEnded());
        }

        private ITreeProvider TreeProvider { get; set; }
        private ISnapshotService SnapshotService { get; set; }
        private ISnapshotReader SnapshotReader { get; set; }
    }
}
