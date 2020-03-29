using System;
using Autofac.Features.Indexed;
using Ctlg.Core;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Services
{
    public class HashingService : IHashingService
    {
        public HashingService(IIndex<string, IHashFunction> hashFunction)
        {
            HashFunctions = hashFunction;
        }

        public HashCalculator CreateHashCalculator(HashAlgorithm algorithm)
        {
            return new HashCalculator(algorithm, GetHashFunction(algorithm.Name));
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
    }
}
