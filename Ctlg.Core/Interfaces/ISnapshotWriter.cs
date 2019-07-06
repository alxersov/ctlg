using System;
using Ctlg.Core;

namespace Ctlg.Core.Interfaces
{
    public interface ISnapshotWriter: IDisposable
    {
        void AddFile(File file);
    }
}
