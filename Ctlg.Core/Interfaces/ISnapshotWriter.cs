using System;
namespace Ctlg.Core.Interfaces
{
    public interface ISnapshotWriter: IDisposable
    {
        void AddFile(File file);
        void AddComment(string comment);
    }
}
