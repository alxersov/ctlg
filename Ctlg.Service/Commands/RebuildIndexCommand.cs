using System;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Commands
{
    public class RebuildIndexCommand : ICommand
    {
        public RebuildIndexCommand(IFileStorageService fileStorageService, IFilesystemService filesystemService,
            IFileStorageIndexService fileStorageIndexService)
        {
            FileStorageService = fileStorageService;
            FilesystemService = filesystemService;
            FileStorageIndexService = fileStorageIndexService;
        }

        public void Execute()
        {
            var currentDirectory = FilesystemService.GetCurrentDirectory();
            var fileStorage = FileStorageService.GetFileStorage(currentDirectory, false);
            var index = FileStorageIndexService.GetIndex(currentDirectory);
            foreach (var hash in fileStorage.GetAllHashes())
            {
                index.Add(hash);
            }
            index.Save();
        }

        private IFileStorageService FileStorageService { get; }
        private IFilesystemService FilesystemService { get; }
        public IFileStorageIndexService FileStorageIndexService { get; }
    }
}
