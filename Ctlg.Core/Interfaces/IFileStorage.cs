using System;
namespace Ctlg.Core.Interfaces
{
    public interface IFileStorage: IDisposable
    {
        BackupFileStatus AddFileToStorage(File file);
        string GetBackupFilePath(string hash);
        void RebuildIndex();
    }
}
