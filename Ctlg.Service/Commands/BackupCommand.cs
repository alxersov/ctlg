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

        public BackupCommand(ITreeProvider treeProvider, ISnapshotService snapshotService, ISnapshotReader snapshotReader, ICtlgService ctlgService)
        {
            TreeProvider = treeProvider;
            SnapshotReader = snapshotReader;
            CtlgService = ctlgService;
        }

        public void Execute()
        {
            var root = TreeProvider.ReadTree(Path, SearchPattern);

            if (IsFastMode)
            {
                SnapshotReader.ReadHashesFromLatestSnapshot(Name, root);
            }

            var treeWalker = new TreeWalker(root);

            using (var writer = CtlgService.CreateBackupWriter(Name, IsFastMode))
            {
                treeWalker.Walk(writer.AddFile);
            }

            DomainEvents.Raise(new BackupCommandEnded());
        }

        private ITreeProvider TreeProvider { get; set; }
        private ICtlgService CtlgService { get; set; }
        private ISnapshotReader SnapshotReader { get; set; }
    }
}
