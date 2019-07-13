using System;
using System.IO;
using System.Text;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;

namespace Ctlg.Service.Commands
{
    public class RestoreCommand: ICommand
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public string Path { get; set; }

        private IFilesystemService FileSystemService { get; }
        private ICtlgService CtlgService { get; }
        private ISnapshotService SnapshotService { get; }

        public RestoreCommand(IFilesystemService fileSystemService, ICtlgService ctlgService, ISnapshotService snapshotService)
        {
            CtlgService = ctlgService;
            SnapshotService = snapshotService;
            FileSystemService = fileSystemService;
        }

        public void Execute(ICtlgService ctlgService)
        {
            var snapshotPath = SnapshotService.FindSnapshotPath(Name, Date);
            var snapshotRecords = SnapshotService.ReadSnapshotFile(snapshotPath);
            foreach (var record in snapshotRecords)
            {
                try
                {
                    ProcessSnapshotRecord(record);
                }
                catch (Exception ex)
                {
                    DomainEvents.Raise(new ErrorEvent(ex));
                }
            }
        }

        private void ProcessSnapshotRecord(SnapshotRecord record)
        {
            var backupFilePath = CtlgService.GetBackupFilePath(record.Hash);
            if (!FileSystemService.FileExists(backupFilePath))
            {
                throw new Exception($"Could not restore {record.Name}. Backup file {backupFilePath} not found.");
            }

            var destinationFile = FileSystemService.CombinePath(Path, record.Name);
            var destinationDir = FileSystemService.GetDirectoryName(destinationFile);
            FileSystemService.CreateDirectory(destinationDir);
            FileSystemService.Copy(backupFilePath, destinationFile);

            DomainEvents.Raise(new BackupEntryRestored(record.Name));
        }
    }
}
