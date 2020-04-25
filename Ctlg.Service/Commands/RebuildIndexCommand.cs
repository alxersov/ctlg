using System;
using Ctlg.Core;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Commands
{
    public class RebuildIndexCommand : ICommand
    {
        public RebuildIndexCommand(IFileStorageService fileStorageService,
            IFileStorageIndexService fileStorageIndexService)
        {
            FileStorageService = fileStorageService;
            FileStorageIndexService = fileStorageIndexService;
        }

        public void Execute(Config config)
        {
            var fileStorage = FileStorageService.GetFileStorage(config.Path, config.HashAlgorithmName);
            var index = FileStorageIndexService.GetIndex(config.Path, config.HashAlgorithmName);
            foreach (var hash in fileStorage.GetAllHashes())
            {
                index.Add(hash);
            }
            index.Save();
        }

        private IFileStorageService FileStorageService { get; }
        public IFileStorageIndexService FileStorageIndexService { get; }
    }
}
