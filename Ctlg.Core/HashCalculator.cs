using System;
using Ctlg.Core.Interfaces;

namespace Ctlg.Core
{
    public class HashCalculator
    {
        public HashCalculator(IHashFunction function, string algorithmName, IFilesystemService filesystemService)
        {
            Function = function;
            FilesystemService = filesystemService;
            Name = algorithmName;
        }

        public string Name { get; private set; }

        public byte[] CalculateHashForFile(string path)
        {
            using (var stream = FilesystemService.OpenFileForRead(path))
            {
                return Function.Calculate(stream);
            }
        }

        public byte[] CalculateHashForFile(string root, string relativePath)
        {
            var path = FilesystemService.CombinePath(root, relativePath);
            return CalculateHashForFile(path);
        }

        private IHashFunction Function { get; }
        private IFilesystemService FilesystemService { get; }
    }
}
