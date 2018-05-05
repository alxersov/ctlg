using System;
namespace Ctlg.Service.Events
{
    public class BackupEntryCreated: IDomainEvent
    {
        public BackupEntryCreated(string backupEntry, bool hashCalculated, bool newFileAddedToStorage)
        {
            BackupEntry = backupEntry;
            HashCalculated = hashCalculated;
            NewFileAddedToStorage = newFileAddedToStorage;
        }

        public string BackupEntry { get; set; }
        public bool HashCalculated { get; set; }
        public bool NewFileAddedToStorage { get; set; }
    }
}
