using System;
using System.Collections.Generic;

namespace Ctlg.Core.Interfaces
{
    public interface ISnapshotFactory
    {
        ISnapshot GetSnapshot(Config config, string name, string timestamp);
        List<string> GetSnapshotNames(Config config);
        List<string> GetTimestamps(Config config, string name);
    }
}
