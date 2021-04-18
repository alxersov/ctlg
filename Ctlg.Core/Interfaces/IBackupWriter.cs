using System;
namespace Ctlg.Core.Interfaces
{
    public interface IBackupWriter : IDisposable
    {
        void AddFile(SnapshotRecord record, IFileStorage sourceStorage);
        void AddFile(File file, string root, byte[] hash);
        void AddComment(string message);
    }
}
