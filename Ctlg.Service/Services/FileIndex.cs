using System;
using System.Collections.Generic;
using System.IO;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Services
{
    public class FileIndex: IFileStorageIndex
    {
        public FileIndex(IFilesystemService filesystemService, string path, int hashLength)
        {
            FilesystemService = filesystemService;
            Path = path;
            HashLength = hashLength;
        }

        public void Add(byte[] hash)
        {
            if (hash.Length != HashLength)
            {
                throw new Exception($"Hash is {hash.Length} bytes. Expected hash to have length {HashLength} bytes.");
            }
            _set.Add(hash);
        }

        public IEnumerable<byte[]> GetAllHashes()
        {
            return _set;
        }

        public bool IsInIndex(byte[] hash)
        {
            return _set.Contains(hash);
        }

        public void Save()
        {
            using (var writer = new BinaryWriter(FilesystemService.CreateFileForWrite(Path)))
            {
                foreach (var hash in _set)
                {
                    writer.Write(hash);
                }
            }
        }

        public void Load()
        {
            try
            {
                using (var reader = new BinaryReader(FilesystemService.OpenFileForRead(Path)))
                {
                    while (true)
                    {
                        var hash = reader.ReadBytes(HashLength);
                        if (hash.Length == HashLength)
                        {
                            _set.Add(hash);
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

        private SortedSet<byte[]> _set = new SortedSet<byte[]>(new ByteArrayComparer());

        private readonly int HashLength;

        private IFilesystemService FilesystemService { get; }
        private string Path { get; }
    }
}
