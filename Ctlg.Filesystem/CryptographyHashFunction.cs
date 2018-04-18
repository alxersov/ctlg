using System.IO;
using Ctlg.Core;
using Ctlg.Core.Interfaces;

namespace Ctlg.Filesystem
{
    /// <summary>
    /// Adapter for <see cref="System.Security.Cryptography.HashAlgorithm"/>. Allows to use any HashAlgorithm as IHashFunction.
    /// </summary>
    public class CryptographyHashFunction<THashAlgorithm> : IHashFunction where THashAlgorithm : System.Security.Cryptography.HashAlgorithm
    {
        public CryptographyHashFunction(THashAlgorithm algorithm, int algorithmId, string name)
        {
            Algorithm = algorithm;
            HashAlgorithmId = algorithmId;
            Name = name;
        }

        public string Name { get; }

        public Hash CalculateHash(Stream stream)
        {
            var hashValue = Algorithm.ComputeHash(stream);
            return new Hash(HashAlgorithmId, hashValue);
        }

        private readonly THashAlgorithm Algorithm;
        private readonly int HashAlgorithmId;
    }
}
