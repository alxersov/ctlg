using System;
using System.Collections.Generic;
using System.IO;

namespace Ctlg.Core.Interfaces
{
    public interface ISnapshotService
    {
        string FindSnapshotPath(string snapshotName, string snapshotDate = null);
        SnapshotFile FindSnapshotFile(string rootPath, string snapshotName, string snapshotDate);
        IEnumerable<SnapshotRecord> ReadSnapshotFile(string path);
        StreamWriter CreateSnapshotWriter(string name, string timestamp = null);
        SnapshotRecord CreateSnapshotRecord(File file);
        File CreateFile(SnapshotRecord record);
    }
}
