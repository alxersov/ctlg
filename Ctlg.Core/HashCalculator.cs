using System;
using System.IO;
using System.Linq;
using Ctlg.Core.Interfaces;

namespace Ctlg.Core
{
    public class HashCalculator
    {
        public HashCalculator(HashAlgorithm algorithm, IHashFunction function, IFilesystemService filesystemService)
        {
            Algorithm = algorithm;
            Function = function;
            FilesystemService = filesystemService;
        }

        public Hash CalculateHashForFile(File file)
        {
            using (var stream = FilesystemService.OpenFileForRead(file.FullPath))
            {
                var hash = Calculate(stream);

                file.Hashes.Add(hash);

                return hash;
            }
        }

        public Hash GetExistingHashValue(File file)
        {
            return file.Hashes.FirstOrDefault(h => h.HashAlgorithmId == Algorithm.HashAlgorithmId);
        }

        private Hash Calculate(Stream stream)
        {
            var value = Function.Calculate(stream);
            return new Hash(Algorithm.HashAlgorithmId, value);
        }

        public HashAlgorithm Algorithm { get; }
        private IHashFunction Function { get; }
        private IFilesystemService FilesystemService { get; }
    }
}
