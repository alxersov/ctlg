using System;
using System.Collections.Generic;

namespace Ctlg.UnitTests.TestDoubles
{
    public class VirtualFilesystemNode
    {
        public VirtualFilesystemNode(string path, string name)
        {
            FullPath = string.IsNullOrEmpty(path) ? name : CombinePath(path, name);
            Name = name;
            Directories = new SortedDictionary<string, VirtualFilesystemNode>();
            Files = new SortedDictionary<string, VirtualFileContent>();
        }

        public SortedDictionary<string, VirtualFilesystemNode> Directories { get; }
        public SortedDictionary<string, VirtualFileContent> Files { get; }
        public string FullPath { get; }
        public string Name { get; }


        public bool DirectoryExists(string name)
        {
            return Directories.ContainsKey(name);
        }

        public VirtualFilesystemNode CreateDirectory(string name)
        {
            if (!DirectoryExists(name))
            {
                Directories.Add(name, new VirtualFilesystemNode(FullPath, name));
            }

            return Directories[name];
        }

        public VirtualFilesystemNode GetDirectory(string name)
        {
            return DirectoryExists(name) ? Directories[name] : null;
        }

        public bool FileExists(string name)
        {
            return Files.ContainsKey(name);
        }

        public VirtualFileContent GetFile(string name)
        {
            return FileExists(name) ? Files[name] : null;
        }

        public static string CombinePath(string path1, string path2)
        {
            return $"{path1}{VirtualFileSystem.Separator}{path2}";
        }
    }
}
