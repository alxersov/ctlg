using System;

namespace Ctlg.Core.Interfaces
{
    public interface ISnapshotService
    {
        File CreateFile(SnapshotRecord record);

        ISnapshot GetSnapshot(string backupRootPath, string hashAlgorithmName, string name, string timestampMask);
        ISnapshot CreateSnapshot(string backupRootPath, string hashAlgorithmName, string name, string timestamp);
    }
}
