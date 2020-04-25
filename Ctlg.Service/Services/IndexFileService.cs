using System;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Services
{
    public sealed class IndexFileService : IFileStorageIndexService
    {
        public IndexFileService(IFilesystemService filesystemService, IDataService dataService)
        {
            FilesystemService = filesystemService;
            DataService = dataService;
        }

        public IFileStorageIndex GetIndex(string backupRootDirectory, string hashAlgorithmName)
        {
            var indexPath = FilesystemService.CombinePath(backupRootDirectory, "index.bin");
            var hashAlgorithm = DataService.GetHashAlgorithm(hashAlgorithmName);

            return new FileIndex(FilesystemService, indexPath, hashAlgorithm.Length);
        }

        private IFilesystemService FilesystemService { get; }
        private IDataService DataService { get; }
    }
}
