using System.Collections.Generic;
using System.IO;
using SharpCompress.Archive;
using File = Ctlg.Data.Model.File;

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
                    yield return new File(entry.Key)
                    {
                        FileCreatedDateTime = entry.CreatedTime,
                        FileModifiedDateTime = entry.LastModifiedTime,
                        Size = entry.Size
                    };
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
