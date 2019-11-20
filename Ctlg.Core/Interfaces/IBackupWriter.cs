using System;

namespace Ctlg.Core.Interfaces
{
    public interface IBackupWriter: IDisposable
    {
        void AddFile(File file);
        void AddComment(string message);
    }
}
