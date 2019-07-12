using System;
using System.Collections.Generic;

namespace Ctlg.Core.Interfaces
{
    public interface ISnapshotService
    {
        IEnumerable<File> GetSnapshotFiles(string snapshotName);
        string FindSnapshotFile(string snapshotName, string snapshotDate = null);
        string GetLastSnapshotPath(string snapshotName);
        IEnumerable<SnapshotRecord> ReadSnapshotFile(string path);
        ISnapshotWriter CreateSnapshotWriter(string name);
    }
}
