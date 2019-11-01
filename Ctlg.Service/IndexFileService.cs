using System;
using System.IO;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service
{
    public class IndexFileService : IIndexFileService
    {
        public IndexFileService(ICtlgService ctlgService, IFilesystemService filesystemService, IIndexService indexService, int hashLength)
        {
            CtlgService = ctlgService;
            FilesystemService = filesystemService;
            IndexService = indexService;
            HashLength = hashLength;
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

        public void Load()
        {
            using (var reader = new BinaryReader(FilesystemService.OpenFileForRead(CtlgService.IndexPath)))
            {
                while (true)
                {
                    var hash = reader.ReadBytes(HashLength);
                    if (hash.Length == HashLength)
                    {
                        IndexService.Add(hash);
                    }
                    else if(hash.Length == 0)
                    {
                        break;
                    }
                    else
                    {
                        throw new Exception($"Corrupted index file.");
                    }
                }
            }
        }

        private ICtlgService CtlgService { get; }
        private IFilesystemService FilesystemService { get; }
        private IIndexService IndexService { get; }
        private readonly int HashLength;
    }
}
