using System;
namespace Ctlg.Core.Interfaces
{
    public interface IBackupService
    {
        IBackupWriter CreateWriter(string directory, bool isFastMode, string hashAlgorithmName,
            string name, string timestamp); 
    }
}
