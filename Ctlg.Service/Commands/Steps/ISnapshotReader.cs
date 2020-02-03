using System;
using Ctlg.Core;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Commands
{
    public interface ISnapshotReader
    {
        void ReadHashesFromSnapshot(ISnapshot snapshot, File destinationTree);
    }
}
