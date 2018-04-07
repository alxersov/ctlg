using System.Collections.Generic;
using System.IO;
using Ctlg.Core;
using SharpCompress.Readers;
using File = Ctlg.Core.File;
using IArchive = Ctlg.Core.Interfaces.IArchive;

namespace Ctlg.Filesystem
{
    public class SharpCompressArchive : IArchive
    {
        public SharpCompressArchive(Stream stream)
        {
            _archive = ReaderFactory.Open(stream);
        }

        public IEnumerable<File> EnumerateEntries()
        {
            while (_archive.MoveToNextEntry())
            {
                var entry = _archive.Entry;
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

        private IReader _archive;
    }
}
