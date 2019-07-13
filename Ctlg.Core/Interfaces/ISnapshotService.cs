using System;
using System.Collections.Generic;

namespace Ctlg.Core.Interfaces
{
    public interface ISnapshotService
    {
        IEnumerable<File> GetSnapshotFiles(string snapshotName);
        string FindSnapshotPath(string snapshotName, string snapshotDate = null);
        IEnumerable<SnapshotRecord> ReadSnapshotFile(string path);
        ISnapshotWriter CreateSnapshotWriter(string name);
    }
}
