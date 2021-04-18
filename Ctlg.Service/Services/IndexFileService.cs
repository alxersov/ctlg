using System;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Services
{
    public sealed class IndexFileService : IFileStorageIndexService
    {
        public IndexFileService(IFilesystemService filesystemService, IHashingService hashingService)
        {
            FilesystemService = filesystemService;
            HashingService = hashingService;
        }

        public IFileStorageIndex GetIndex(string backupRootDirectory, string hashAlgorithmName)
        {
            var indexPath = FilesystemService.CombinePath(backupRootDirectory, "index.bin");
            var hashFunction = HashingService.GetHashFunction(hashAlgorithmName);

            return new FileIndex(FilesystemService, indexPath, hashFunction.HashSize);
        }

        private IFilesystemService FilesystemService { get; }
        private IHashingService HashingService { get; }
    }
}
