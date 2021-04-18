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

        public Hash CalculateHashForFile(string path)
        {
            using (var stream = FilesystemService.OpenFileForRead(path))
            {
                return Calculate(stream);
            }
        }

        public Hash CalculateHashForFile(string root, string relativePath)
        {
            var path = FilesystemService.CombinePath(root, relativePath);
            return CalculateHashForFile(path);
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
