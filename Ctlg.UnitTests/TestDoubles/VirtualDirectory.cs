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
        }

        public string Name
        {
            get
            {
                return Node?.Name;
            }
        }

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
                    FullPath = VirtualFilesystemNode.CombinePath(Node.FullPath, file.Key),
                    FileModifiedDateTime = file.Value.FileModifiedDateTime,
                    Size = file.Value.GetSize()
                };
            }
        }

        private VirtualFilesystemNode Node { get; }
    }
}
