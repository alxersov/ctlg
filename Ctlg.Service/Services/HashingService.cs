using System;
using Autofac.Features.Indexed;
using Ctlg.Core;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Services
{
    public class HashingService : IHashingService
    {
        public HashingService(IIndex<string, IHashFunction> hashFunctions, IFilesystemService filesystemService)
        {
            HashFunctions = hashFunctions;
            FilesystemService = filesystemService;
        }

        public HashCalculator CreateHashCalculator(string algorithmName)
        {
            var canonicalName = algorithmName.ToUpperInvariant();
            var hashFunction = GetHashFunction(canonicalName);

            return new HashCalculator(hashFunction, canonicalName, FilesystemService);
        }


        private IHashFunction GetHashFunction(string canonicalName)
        {
            if (!HashFunctions.TryGetValue(canonicalName, out IHashFunction hashFunction))
            {
                throw new Exception($"Unsupported hash function {canonicalName}");
            }

            return hashFunction;
        }

        private IIndex<string, IHashFunction> HashFunctions { get; set; }
        private IFilesystemService FilesystemService { get; }
    }
}
