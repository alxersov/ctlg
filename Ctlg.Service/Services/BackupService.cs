using System;
using Ctlg.Core;
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

        public IBackupWriter CreateWriter(Config config, string name, string timestamp, bool isFastMode)
        {
            var index = IndexService.GetIndex(config.Path, config.HashAlgorithmName);
            var fileStorage = FileStorageService.GetFileStorage(config.Path, config.HashAlgorithmName);
            var snapshot = SnapshotService.CreateSnapshot(config, name, timestamp);

            var hashCalculator = HashingService.CreateHashCalculator(config.HashAlgorithmName);

            return new BackupWriter(fileStorage, snapshot.GetWriter(), isFastMode, index, hashCalculator);
        }

        private IFileStorageService FileStorageService { get; }
        private ISnapshotService SnapshotService { get; }
        private IFileStorageIndexService IndexService { get; }
        private IHashingService HashingService { get; }
    }
}
