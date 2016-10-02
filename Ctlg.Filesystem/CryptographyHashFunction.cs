using System.IO;
using System.Security.Cryptography;
using Ctlg.Core.Interfaces;

namespace Ctlg.Filesystem
{
    /// <summary>
    /// Adapter for <see cref="System.Security.Cryptography.HashAlgorithm"/>. Allows to use any HashAlgorithm as IHashFunction.
    /// </summary>
    public class CryptographyHashFunction<THashAlgorithm> : IHashFunction where THashAlgorithm : HashAlgorithm
    {
        public CryptographyHashFunction(THashAlgorithm algorithm, string name)
        {
            Algorithm = algorithm;
            Name = name;
        }

        public string Name { get; }

        public byte[] CalculateHash(Stream stream)
        {
            return Algorithm.ComputeHash(stream);
        }

        private readonly THashAlgorithm Algorithm;
    }
}
