using System;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Commands
{
    public class RebuildIndexCommand : ICommand
    {
        public RebuildIndexCommand(IFileStorageService fileStorageService, IFilesystemService filesystemService)
        {
            FileStorageService = fileStorageService;
            FilesystemService = filesystemService;
        }

        public void Execute()
        {
            var currentDirectory = FilesystemService.GetCurrentDirectory();
            using (var fileStorage = FileStorageService.GetFileStorage(currentDirectory, false, false))
            {
                fileStorage.RebuildIndex();
            }
        }

        private IFileStorageService FileStorageService { get; }
        private IFilesystemService FilesystemService { get; }
    }
}
