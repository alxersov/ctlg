using System;
namespace Ctlg.Core.Interfaces
{
    public interface ISnapshotWriter: IDisposable
    {
        void AddFile(File file, byte[] hash);
        void AddComment(string comment);
    }
}
