using System;
namespace Ctlg.Service.Events
{
    public class BackupCommandStarted : IDomainEvent
    {
		public string SnapshotFile { get; set; }
        public string FileStorage { get; set; }

        public BackupCommandStarted(string snapshotFile, string fileStorage)
        {
            SnapshotFile = snapshotFile;
            FileStorage = fileStorage;
        }
    }
}
