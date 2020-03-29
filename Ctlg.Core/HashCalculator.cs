using System;
using System.IO;
using Ctlg.Core.Interfaces;

namespace Ctlg.Core
{
    public class HashCalculator
    {
        public HashCalculator(HashAlgorithm algorithm, IHashFunction function)
        {
            Algorithm = algorithm;
            Function = function;
        }

        public Hash CalculateHashForFile(File file, IFilesystemService filesystemService)
        {
            using (var stream = filesystemService.OpenFileForRead(file.FullPath))
            {
                var hash = Calculate(stream);

                file.Hashes.Add(hash);

                return hash;
            }
        }

        Hash Calculate(Stream stream)
        {
            var value = Function.Calculate(stream);
            return new Hash(Algorithm.HashAlgorithmId, value);
        }

        private HashAlgorithm Algorithm { get; }
        private IHashFunction Function { get; }
    }
}
