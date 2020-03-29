using System;
using System.IO;
using Ctlg.Core.Interfaces;

namespace Ctlg.Filesystem
{
    /// <summary>
    /// Adapter for <see cref="System.Security.Cryptography.HashAlgorithm"/>. Allows to use any HashAlgorithm as IHashFunction.
    /// </summary>
    public class CryptographyHashFunction<THashAlgorithm> : IHashFunction where THashAlgorithm : System.Security.Cryptography.HashAlgorithm
    {
        public CryptographyHashFunction(THashAlgorithm algorithm)
        {
            Algorithm = algorithm;
        }

        public byte[] Calculate(Stream stream)
        {
            return Algorithm.ComputeHash(stream);
        }

        private readonly THashAlgorithm Algorithm;
    }
}
