using System;
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
        private IFileStorageService FileStorageService { get; }
        private ISnapshotService SnapshotService { get; }

        public RestoreCommand(IFilesystemService fileSystemService, IFileStorageService fileStorageService, ISnapshotService snapshotService)
        {
            FileStorageService = fileStorageService;
            SnapshotService = snapshotService;
            FileSystemService = fileSystemService;
        }

        public void Execute()
        {
            var currentDir = FileSystemService.GetCurrentDirectory();
            var snapshot = SnapshotService.GetSnapshot(currentDir, Name, Date);
            if (snapshot == null)
            {
                throw new Exception($"Snapshot {Name} is not found");
            }

            var fileStorage = FileStorageService.GetFileStorage(currentDir, true);
            var snapshotRecords = snapshot.EnumerateFiles();
            foreach (var record in snapshotRecords)
            {
                try
                {
                    ProcessSnapshotRecord(fileStorage, record);
                }
                catch (Exception ex)
                {
                    DomainEvents.Raise(new ErrorEvent(ex));
                }
            }
        }

        private void ProcessSnapshotRecord(IFileStorage fileStorage, SnapshotRecord record)
        {
            var destinationPath = FileSystemService.CombinePath(Path, record.Name);
            fileStorage.CopyFileTo(record.Hash, destinationPath);

            DomainEvents.Raise(new BackupEntryRestored(record.Name));
        }
    }
}
