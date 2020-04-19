using System;
using System.Collections.Generic;
using Ctlg.Core;
using Ctlg.Core.Interfaces;

namespace Ctlg.UnitTests.TestDoubles
{
    public class VirtualDirectory : IFilesystemDirectory
    {
        public VirtualDirectory(VirtualFilesystemNode node)
        {
            Node = node;

            Directory = new File
            {
                IsDirectory = true,
                Name = Node.Name,
                FullPath = node.FullPath,
                FileCreatedDateTime = DateTime.UtcNow,
                FileModifiedDateTime = DateTime.UtcNow,
                RecordUpdatedDateTime = DateTime.UtcNow
            };
        }

        public File Directory { get; set; }

        public IEnumerable<IFilesystemDirectory> EnumerateDirectories()
        {
            foreach (var node in Node.Directories.Values)
            {
                yield return new VirtualDirectory(node);
            }
        }

        public IEnumerable<File> EnumerateFiles(string searchPattern)
        {
            foreach (var file in Node.Files)
            {
                yield return new File(file.Key)
                {
                    FullPath = VirtualFilesystemNode.CombinePath(Directory.FullPath, file.Key),
                    FileCreatedDateTime = file.Value.FileCreatedDateTime,
                    FileModifiedDateTime = file.Value.FileModifiedDateTime,
                    Size = file.Value.GetSize(),
                    RecordUpdatedDateTime = DateTime.UtcNow
                };
            }
        }

        private VirtualFilesystemNode Node { get; }
    }
}
