using System;
using System.Linq;
using Ctlg.Core;
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

        public IFileStorage GetFileStorage(string backupRootDirectory, bool shouldUseIndex)
        {
            return new SimpleFileStorage(FilesystemService, HashingService, backupRootDirectory);
        }

        private IFilesystemService FilesystemService { get; }
        public IHashingService HashingService { get; }
        public IFileStorageIndexService IndexService { get; }
    }
}
