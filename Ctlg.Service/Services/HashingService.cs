using System;
using Autofac.Features.Indexed;
using Ctlg.Core;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Services
{
    public class HashingService : IHashingService
    {
        public HashingService(IFilesystemService filesystemService, IIndex<string, IHashFunction> hashFunction)
        {
            FilesystemService = filesystemService;
            HashFunctions = hashFunction;
        }

        public IHashFunction GetHashFunction(string name)
        {
            var canonicalName = name.ToUpperInvariant();
            if (!HashFunctions.TryGetValue(canonicalName, out IHashFunction hashFunction))
            {
                throw new Exception($"Unsupported hash function {name}");
            }

            return hashFunction;
        }

        public Hash CalculateHashForFile(File file, IHashFunction hashFunction)
        {
            using (var stream = FilesystemService.OpenFileForRead(file.FullPath))
            {
                var hash = hashFunction.CalculateHash(stream);

                file.Hashes.Add(hash);

                return hash;
            }
        }

        private IFilesystemService FilesystemService { get; }
        private IIndex<string, IHashFunction> HashFunctions { get; set; }
    }
}
