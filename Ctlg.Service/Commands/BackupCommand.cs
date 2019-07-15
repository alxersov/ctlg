using System;
using Ctlg.Core;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Commands
{
    public class BackupCommand: ICommand
    {
        public string SnapshotName { get; set; }
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
                SnapshotReader.ReadHashesFromLatestSnapshot(SnapshotName, root);
            }

            var treeWalker = new TreeWalker(root);

            using (var snapshot = SnapshotService.CreateSnapshotWriter(SnapshotName))
            {
                treeWalker.Walk(snapshot.AddFile);
            }
        }

        private ITreeProvider TreeProvider { get; set; }
        private ISnapshotService SnapshotService { get; set; }
        private ISnapshotReader SnapshotReader { get; set; }
    }
}
