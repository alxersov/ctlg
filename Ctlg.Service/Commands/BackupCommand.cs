using System;
using Ctlg.Core;

namespace Ctlg.Service.Commands
{
    public class BackupCommand: ICommand
    {
        public string SnapshotName { get; set; }
        public string Path { get; set; }
        public string SearchPattern { get; set; }
        public bool IsFastMode { get; set; }

        public BackupCommand(ITreeProvider treeProvider, ISnapshotWriterProvider snapshotWriterProvider, ISnapshotReader snapshotReader)
        {
            TreeProvider = treeProvider;
            SnapshotWriterProvider = snapshotWriterProvider;
            SnapshotReader = snapshotReader;
        }

        public void Execute(ICtlgService ctlgService)
        {
            var root = TreeProvider.ReadTree(Path, SearchPattern);

            if (IsFastMode)
            {
                SnapshotReader.ReadHashesFromLatestSnapshot(SnapshotName, root);
            }

            var treeWalker = new TreeWalker(root);

            using (var snapshot = SnapshotWriterProvider.CreateSnapshotWriter(SnapshotName))
            {
                treeWalker.Walk(snapshot.AddFile);
            }
        }

        private ITreeProvider TreeProvider { get; set; }
        private ISnapshotWriterProvider SnapshotWriterProvider { get; set; }
        private ISnapshotReader SnapshotReader { get; set; }
    }
}
