using System;
using Autofac.Features.Indexed;
using Ctlg.Core;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Services
{
    public class HashingService : IHashingService
    {
        public HashingService(IIndex<string, IHashFunction> hashFunction, IFilesystemService filesystemService,
            IDataService dataService)
        {
            HashFunctions = hashFunction;
            FilesystemService = filesystemService;
            DataService = dataService;
        }

        public HashCalculator CreateHashCalculator(string algorithmName)
        {
            return CreateHashCalculator(DataService.GetHashAlgorithm(algorithmName));
        }

        public HashCalculator CreateHashCalculator(HashAlgorithm algorithm)
        {
            return new HashCalculator(algorithm, GetHashFunction(algorithm.Name), FilesystemService);
        }

        private IHashFunction GetHashFunction(string name)
        {
            var canonicalName = name.ToUpperInvariant();
            if (!HashFunctions.TryGetValue(canonicalName, out IHashFunction hashFunction))
            {
                throw new Exception($"Unsupported hash function {name}");
            }

            return hashFunction;
        }

        private IIndex<string, IHashFunction> HashFunctions { get; set; }
        private IFilesystemService FilesystemService { get; }
        private IDataService DataService { get; }
    }
}
