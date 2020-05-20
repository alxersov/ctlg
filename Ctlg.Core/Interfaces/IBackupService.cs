using System;
namespace Ctlg.Core.Interfaces
{
    public interface IBackupService
    {
        IBackupWriter CreateWriter(Config config, string name, string timestamp, bool isFastMode); 
    }
}
