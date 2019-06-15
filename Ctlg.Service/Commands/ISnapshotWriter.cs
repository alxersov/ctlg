using System;
using Ctlg.Core;

namespace Ctlg.Service.Commands
{
    public interface ISnapshotWriter: IDisposable
    {
        void AddFile(File file);
    }
}
