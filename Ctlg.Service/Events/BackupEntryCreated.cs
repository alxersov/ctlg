using System;
using Ctlg.Core;

namespace Ctlg.Service.Events
{
    public class BackupEntryCreated: IDomainEvent
    {
        public BackupEntryCreated(SnapshotRecord backupEntry, bool hashCalculated, bool isHashFoundInIndex, bool newFileAddedToStorage)
        {
            BackupEntry = backupEntry;
            HashCalculated = hashCalculated;
            IsHashFoundInIndex = isHashFoundInIndex;
            NewFileAddedToStorage = newFileAddedToStorage;
        }

        public SnapshotRecord BackupEntry { get; set; }
        public bool HashCalculated { get; set; }
        public bool IsHashFoundInIndex { get; set; }
        public bool NewFileAddedToStorage { get; set; }
    }
}
