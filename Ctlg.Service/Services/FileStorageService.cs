using System;
using System.Linq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Services
{
    public sealed class FileStorageService: IFileStorageService
    {
        public FileStorageService(IFilesystemService filesystemService, ICtlgService ctlgService, IFileStorageIndexService indexService)
        {
            FilesystemService = filesystemService;
            CtlgService = ctlgService;
            IndexService = indexService;
        }

        public IFileStorage GetFileStorage(string backupRootDirectory, bool shouldUseIndex, bool shouldExistingHashMatchCaclulated)
        {
            return new FileStorage(FilesystemService, CtlgService, IndexService,
                backupRootDirectory, shouldUseIndex, shouldExistingHashMatchCaclulated);
        }

        private IFilesystemService FilesystemService { get; }
        public ICtlgService CtlgService { get; }
        public IFileStorageIndexService IndexService { get; }
    }
}
