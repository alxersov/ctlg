using System;
using Ctlg.Core;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Commands
{
    public class SnapshotReader
    {
        public void ReadHashesFromSnapshot(ISnapshot snapshot, File destinationTree)
        {
            destinationTree.SortTree();

            foreach (var record in snapshot.EnumerateFiles())
            {
                ProcessRecord(record, destinationTree);
            }
        }

        private void ProcessRecord(File record, File root)
        {
            var path = record.Name.Split('\\', '/');

            var i = 0;
            File currentFile = root;
            while (i < path.Length && currentFile != null)
            {
                currentFile = currentFile.GetInnerFile(path[i]);
                ++i;
            }

            if (currentFile != null &&
                currentFile.Size == record.Size &&
                currentFile.FileModifiedDateTime == record.FileModifiedDateTime)
            {
                currentFile.Hashes.AddRange(record.Hashes);
            }
        }
    }
}
