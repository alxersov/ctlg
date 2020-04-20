using System;
namespace Ctlg.Core.Interfaces
{
    public interface IBackupWriter : IDisposable
    {
        void AddFile(File file, IFileStorage sourceStorage = null);
        void AddComment(string message);
    }
}
