using System;
using System.IO;

namespace Ctlg.Service.Commands
{
    public interface ISnapshotWriterProvider
    {
        ISnapshotWriter CreateSnapshotWriter(string name);
    }
}
