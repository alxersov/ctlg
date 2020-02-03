using System;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Services
{
    public sealed class IndexFileService : IFileStorageIndexService
    {
        public IndexFileService(IFilesystemService filesystemService)
        {
            FilesystemService = filesystemService;
        }

        public IFileStorageIndex GetIndex(string backupRootDirectory)
        {
            var indexPath = FilesystemService.CombinePath(backupRootDirectory, "index.bin");
            return new FileIndex(FilesystemService, indexPath, 32);
        }

        private IFilesystemService FilesystemService { get; }
    }
}
