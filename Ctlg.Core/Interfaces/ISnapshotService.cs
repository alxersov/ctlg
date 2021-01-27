using System;
using System.Collections.Generic;

namespace Ctlg.Core.Interfaces
{
    public interface ISnapshotService
    {
        ISnapshot FindSnapshot(Config config, string name, string timestampMask);
        ISnapshot CreateSnapshot(Config config, string name, string timestamp);
        List<string> GetSnapshotNames(Config config);
        List<string> GetTimestamps(Config config, string name);
    }
}
