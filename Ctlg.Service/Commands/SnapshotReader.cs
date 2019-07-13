using System;
using StreamReader = System.IO.StreamReader;
using Ctlg.Core;
using Ctlg.Service.Events;
using Ctlg.Service.Utils;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Commands
{
    public class SnapshotReader : ISnapshotReader
    {
        public SnapshotReader(ICtlgService ctlgService, ISnapshotService snapshotService)
        {
            CtlgService = ctlgService;
            SnapshotService = snapshotService;
        }

        public ICtlgService CtlgService { get; }
        public ISnapshotService SnapshotService { get; }

        public void ReadHashesFromLatestSnapshot(string snapshotName, File destinationTree)
        {
            var snapshotPath = SnapshotService.FindSnapshotPath(snapshotName);
            if (snapshotPath == null)
            {
                return;
            }

            CtlgService.SortTree(destinationTree);

            var snapshotRecords = SnapshotService.ReadSnapshotFile(snapshotPath);
            foreach (var record in snapshotRecords)
            {
                ProcessRecord(record, destinationTree);
            }
        }

        private void ProcessRecord(SnapshotRecord record, Core.File root)
        {
            var path = record.Name.Split('\\', '/');

            var i = 0;
            File currentFile = root;
            while (i < path.Length && currentFile != null)
            {
                currentFile = CtlgService.GetInnerFile(currentFile, path[i]);
                ++i;
            }

            if (i == path.Length &&
                currentFile.Size == record.Size &&
                currentFile.FileModifiedDateTime == record.Date)
            {
                currentFile.Hashes.Add(new Hash(HashAlgorithmId.SHA256, FormatBytes.ToByteArray(record.Hash)));
            }
        }
    }
}
