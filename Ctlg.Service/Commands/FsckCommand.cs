using System;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;

namespace Ctlg.Service.Commands
{
    public class FsckCommand: ICommand
    {
        public FsckCommand(IFileStorageService fileStorageService, ISnapshotService snapshotService)
        {
            FileStorageService = fileStorageService;
            SnapshotService = snapshotService;
        }

        public void Execute(Config config)
        {
            var fileStorage = FileStorageService.GetFileStorage(config.Path, config.HashAlgorithmName);
            foreach (var hash in fileStorage.GetAllHashes())
            {
                try
                {
                    fileStorage.VerifyFileByHash(hash);
                }
                catch (Exception e)
                {
                    DomainEvents.Raise(new ErrorEvent(e));
                }
            }

            var snapshotNames = SnapshotService.GetSnapshotNames(config);
            foreach (var snapshotName in snapshotNames)
            {
                var timestamps = SnapshotService.GetTimestamps(config, snapshotName);
                foreach (var timestamp in timestamps)
                {
                    DomainEvents.Raise(new EnumeratingSnapshots(snapshotName, timestamp));
                    var snapshot = SnapshotService.FindSnapshot(config, snapshotName, timestamp);
                    foreach (var file in snapshot.EnumerateFiles())
                    {
                        try
                        {
                            if (!fileStorage.IsFileInStorage(file))
                            {
                                throw new Exception($"File {file.Name} is not found in storage.");
                            }
                        }
                        catch (Exception e)
                        {
                            DomainEvents.Raise(new ErrorEvent(e));
                        }
                    }
                }
            }
        }

        private IFileStorageService FileStorageService { get; }
        private ISnapshotService SnapshotService { get; }
    }
}
