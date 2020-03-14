using System;
namespace Ctlg.Core.Interfaces
{
    public interface IBackupWriter : IDisposable
    {
        IFileStorage Storage { get; }
        void AddFile(File file, IFileStorage sourceStorage = null);
        void AddComment(string message);
    }
}
