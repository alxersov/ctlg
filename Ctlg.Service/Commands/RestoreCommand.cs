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

        private IFilesystemService FilesystemService { get; }
        private IFileStorageService FileStorageService { get; }
        private ISnapshotService SnapshotService { get; }

        public RestoreCommand(IFilesystemService filesystemService, IFileStorageService fileStorageService,
            ISnapshotService snapshotService)
        {
            FilesystemService = filesystemService;
            FileStorageService = fileStorageService;
            SnapshotService = snapshotService;
        }

        public void Execute(Config config)
        {
            var snapshot = SnapshotService.GetSnapshot(config.Path, config.HashAlgorithmName, Name, Date);
            if (snapshot == null)
            {
                throw new Exception($"Snapshot {Name} is not found");
            }

            var fileStorage = FileStorageService.GetFileStorage(config.Path, config.HashAlgorithmName);
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

        private void ProcessSnapshotRecord(IFileStorage fileStorage, File record)
        {
            var destinationPath = FilesystemService.CombinePath(Path, record.Name);
            fileStorage.CopyFileTo(record, destinationPath);

            DomainEvents.Raise(new BackupEntryRestored(record.Name));
        }
    }
}
