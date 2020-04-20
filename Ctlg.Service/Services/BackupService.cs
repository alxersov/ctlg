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

        public IBackupWriter CreateWriter(string directory, bool isFastMode, string hashAlgorithmName,
            string name, string timestamp)
        {
            var index = IndexService.GetIndex(directory);
            var fileStorage = FileStorageService.GetFileStorage(directory, hashAlgorithmName);
            var snapshot = SnapshotService.CreateSnapshot(directory, hashAlgorithmName, name, timestamp);

            var hashCalculator = HashingService.CreateHashCalculator(hashAlgorithmName);
            return new BackupWriter(fileStorage, snapshot.GetWriter(), isFastMode, index, hashCalculator);
        }

        private IFileStorageService FileStorageService { get; }
        private ISnapshotService SnapshotService { get; }
        private IFileStorageIndexService IndexService { get; }
        private IHashingService HashingService { get; }
    }
}
