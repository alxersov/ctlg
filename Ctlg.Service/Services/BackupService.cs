using System;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Services
{
    public class BackupService : IBackupService
    {
        public BackupService(IFileStorageService fileStorageService, ISnapshotService snapshotService,
            IFileStorageIndexService indexService, IHashingService hashingService)
        {
            FileStorageService = fileStorageService;
            SnapshotService = snapshotService;
            IndexService = indexService;
            HashingService = hashingService;
        }

        public IFileStorageService FileStorageService { get; }
        public ISnapshotService SnapshotService { get; }
        public IFileStorageIndexService IndexService { get; }
        public IHashingService HashingService { get; }

        public IBackupWriter CreateWriter(string directory, bool isFastMode, string name, string timestamp)
        {
            var index = IndexService.GetIndex(directory);
            var fileStorage = FileStorageService.GetFileStorage(directory, isFastMode);
            var snapshot = SnapshotService.CreateSnapshot(directory, name, timestamp);

            return new BackupWriter(fileStorage, snapshot.GetWriter(), isFastMode, index, HashingService);
        }
    }
}
