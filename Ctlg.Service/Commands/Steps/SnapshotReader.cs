using System;
using Ctlg.Core;
using Ctlg.Service.Utils;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Commands
{
    public class SnapshotReader : ISnapshotReader
    {
        public SnapshotReader(ICtlgService ctlgService)
        {
            CtlgService = ctlgService;
        }

        public ICtlgService CtlgService { get; }
        public ISnapshotService SnapshotService { get; }

        public void ReadHashesFromSnapshot(ISnapshot snapshot, File destinationTree)
        {
            CtlgService.SortTree(destinationTree);

            foreach (var record in snapshot.EnumerateFiles())
            {
                ProcessRecord(record, destinationTree);
            }
        }

        private void ProcessRecord(SnapshotRecord record, File root)
        {
            var path = record.Name.Split('\\', '/');

            var i = 0;
            File currentFile = root;
            while (i < path.Length && currentFile != null)
            {
                currentFile = CtlgService.GetInnerFile(currentFile, path[i]);
                ++i;
            }

            if (currentFile != null &&
                currentFile.Size == record.Size &&
                currentFile.FileModifiedDateTime == record.Date)
            {
                currentFile.Hashes.Add(new Hash(HashAlgorithmId.SHA256, FormatBytes.ToByteArray(record.Hash)));
            }
        }
    }
}
