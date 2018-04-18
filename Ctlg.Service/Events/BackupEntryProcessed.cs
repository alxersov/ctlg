using System;
namespace Ctlg.Service.Events
{
    public class BackupEntryProcessed: IDomainEvent
    {
        public BackupEntryProcessed(string backupEntry)
        {
            BackupEntry = backupEntry;
        }

        public string BackupEntry { get; set; }
    }
}
