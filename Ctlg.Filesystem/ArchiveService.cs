using System;
using System.IO;
using System.Linq;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service
{
    public class ArchiveService: IArchiveService
    {
        public ArchiveService(IDataService dataService)
        {
            DataService = dataService;
        }

        public bool IsArchiveExtension(string path)
        {
            var ext = Path.GetExtension(path);

            return ArchiveExtensions.Any(e => e.Equals(ext, StringComparison.OrdinalIgnoreCase));
        }

        public IArchive OpenArchive(Stream stream)
        {
            return new Filesystem.SharpCompressArchive(stream, DataService.GetHashAlgorithm("CRC32"));
        }

        private static readonly string[] ArchiveExtensions = { ".ZIP", ".7Z", ".RAR" };

        private IDataService DataService { get; }
    }
}
