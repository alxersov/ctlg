using System;
namespace Ctlg.Core.Interfaces
{
    public interface IBackupService
    {
        IBackupWriter CreateWriter(string directory, bool isFastMode, string name, string timestamp); 
    }
}
