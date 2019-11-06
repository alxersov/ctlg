using System;
using Ctlg.Core;

namespace Ctlg.Service.Commands
{
    public interface ISnapshotReader
    {
        void ReadHashesFromLatestSnapshot(string snapshotName, File destinationTree);
    }
}
