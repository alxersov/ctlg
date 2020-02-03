using System;
namespace Ctlg.Core.Interfaces
{
    public interface ISnapshotWriter: IDisposable
    {
        SnapshotRecord AddFile(File file);
        void AddComment(string comment);
    }
}
