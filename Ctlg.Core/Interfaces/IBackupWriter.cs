using System;
namespace Ctlg.Core.Interfaces
{
    public interface IBackupWriter : IDisposable
    {
        void AddFile(File file, byte[] hash, IFileStorage sourceStorage);
        void AddFile(File file, byte[] hash);
        void AddComment(string message);
    }
}
