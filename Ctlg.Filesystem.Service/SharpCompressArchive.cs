using System.Collections.Generic;
using System.IO;
using Ctlg.Core;
using SharpCompress.Archive;
using File = Ctlg.Core.File;

namespace Ctlg.Filesystem.Service
{
    public class SharpCompressArchive : IArchive
    {
        public SharpCompressArchive(Stream stream)
        {
            _archive = ArchiveFactory.Open(stream);
        }

        public IEnumerable<File> EnumerateEntries()
        {
            foreach (var entry in _archive.Entries)
            {
                if (!entry.IsDirectory)
                {
                    var file = new File(entry.Key)
                    {
                        FileCreatedDateTime = entry.CreatedTime,
                        FileModifiedDateTime = entry.LastModifiedTime,
                        Size = entry.Size
                    };

                    file.Hashes.Add(new Hash(HashAlgorithmId.CRC32, (uint)entry.Crc));

                    yield return file;
                }
            }
        }

        public void Dispose()
        {
            if (_archive != null)
            {
                _archive.Dispose();
                _archive = null;
            }
        }

        private SharpCompress.Archive.IArchive _archive;
    }
}
