using System;
namespace Ctlg.Core.Interfaces
{
    public interface IFileStorageService
    {
        string GetBackupFilePath(string hash);
        void AddFileToStorage(File file);
        bool IsFileInStorage(File file);
        string FileStorageDirectory { get; }
    }
}
