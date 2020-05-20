using System;

namespace Ctlg.Core.Interfaces
{
    public interface ISnapshotService
    {
        ISnapshot FindSnapshot(Config config, string name, string timestampMask);
        ISnapshot CreateSnapshot(Config config, string name, string timestamp);
    }
}
