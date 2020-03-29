using System;
using Ctlg.Core.Interfaces;
using Ctlg.Service.FileStorage;

namespace Ctlg.Service.Services
{
    public sealed class FileStorageService: IFileStorageService
    {
        public FileStorageService(IFilesystemService filesystemService, IHashingService hashingService, IDataService dataService)
        {
            FilesystemService = filesystemService;
            HashingService = hashingService;
            DataService = dataService;
        }

        public IFileStorage GetFileStorage(string backupRootDirectory, bool shouldUseIndex)
        {
            return new SimpleFileStorage(FilesystemService, HashingService, DataService, backupRootDirectory);
        }

        private IFilesystemService FilesystemService { get; }
        public IHashingService HashingService { get; }
        public IDataService DataService { get; }
        public IFileStorageIndexService IndexService { get; }
    }
}
