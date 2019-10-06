using System;
using System.IO;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service
{
    public class IndexFileService : IIndexFileService
    {
        public IndexFileService(ICtlgService ctlgService, IFilesystemService filesystemService, IIndexService indexService)
        {
            CtlgService = ctlgService;
            FilesystemService = filesystemService;
            IndexService = indexService;
        }

        public void Save()
        {
            using (var writer = new BinaryWriter(FilesystemService.CreateFileForWrite(CtlgService.IndexPath)))
            {
                foreach (var hash in IndexService.GetAllHashes())
                {
                    writer.Write(hash);
                }
            }
        }

        private ICtlgService CtlgService { get; }
        private IFilesystemService FilesystemService { get; }
        private IIndexService IndexService { get; }
    }
}
