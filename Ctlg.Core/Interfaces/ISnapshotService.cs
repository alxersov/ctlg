using System;
using System.Collections.Generic;
using System.IO;

namespace Ctlg.Core.Interfaces
{
    public interface ISnapshotService
    {
        IEnumerable<File> GetSnapshotFiles(string snapshotName);
        string FindSnapshotPath(string snapshotName, string snapshotDate = null);
        IEnumerable<SnapshotRecord> ReadSnapshotFile(string path);
        StreamWriter CreateSnapshotWriter(string name);
        SnapshotRecord CreateSnapshotRecord(File file);
    }
}
