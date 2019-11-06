using System;
using System.IO;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Services
{
    public sealed class IndexFileService : IIndexFileService
    {
        public IndexFileService(IFilesystemService filesystemService, IIndexService indexService, int hashLength)
        {
            FilesystemService = filesystemService;
            IndexService = indexService;
            HashLength = hashLength;

            var currentDirectory = FilesystemService.GetCurrentDirectory();
            IndexPath = FilesystemService.CombinePath(currentDirectory, "index.bin");
        }

        public void Save()
        {
            using (var writer = new BinaryWriter(FilesystemService.CreateFileForWrite(IndexPath)))
            {
                foreach (var hash in IndexService.GetAllHashes())
                {
                    writer.Write(hash);
                }
            }
        }

        public void Load()
        {
            try
            {
                using (var reader = new BinaryReader(FilesystemService.OpenFileForRead(IndexPath)))
                {
                    while (true)
                    {
                        var hash = reader.ReadBytes(HashLength);
                        if (hash.Length == HashLength)
                        {
                            IndexService.Add(hash);
                        }
                        else if (hash.Length == 0)
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
            catch (FileNotFoundException)
            {
                // Ignore exception. Index file does not exist on the first run.
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string IndexPath { get; set; }

        private IFilesystemService FilesystemService { get; }
        private IIndexService IndexService { get; }
        private readonly int HashLength;
    }
}
