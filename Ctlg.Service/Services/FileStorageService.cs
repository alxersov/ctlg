using System;
using Ctlg.Core.Interfaces;
using Ctlg.Service.FileStorage;

namespace Ctlg.Service.Services
{
    public sealed class FileStorageService: IFileStorageService
    {
        public FileStorageService(IFilesystemService filesystemService, IHashingService hashingService)
        {
            FilesystemService = filesystemService;
            HashingService = hashingService;
        }

        public IFileStorage GetFileStorage(string backupRootDirectory, string hashAlgorithmName)
        {
            var hashCalculator = HashingService.CreateHashCalculator(hashAlgorithmName);

            return new SimpleFileStorage(FilesystemService, hashCalculator, backupRootDirectory);
        }

        private IFilesystemService FilesystemService { get; }
        private IHashingService HashingService { get; }
    }
}
