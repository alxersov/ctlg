using System;
using Ctlg.Core;

namespace Ctlg.Core.Interfaces
{
    public interface IBackupWriter: IDisposable
    {
        void AddFile(File file);
    }
}
