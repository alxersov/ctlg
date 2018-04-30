using System;
namespace Ctlg.Service.Events
{
    public class BackupEntryCreated: IDomainEvent
    {
        public BackupEntryCreated(string backupEntry)
        {
            BackupEntry = backupEntry;
        }

        public string BackupEntry { get; set; }
    }
}
