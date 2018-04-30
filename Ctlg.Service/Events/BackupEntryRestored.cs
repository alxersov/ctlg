using System;
namespace Ctlg.Service.Events
{
    public class BackupEntryRestored: IDomainEvent
    {
        public BackupEntryRestored(string backupEntry)
        {
            BackupEntry = backupEntry;
        }

        public string BackupEntry { get; set; }
    }
}
