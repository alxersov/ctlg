using System;
using System.Collections.Generic;
using System.IO;
using Ctlg.Core.Interfaces;

namespace Ctlg.UnitTests.TestDoubles
{
    public class VirtualFileSystem : IFilesystemService
    {
        public VirtualFileSystem()
        {
        }

        public string CombinePath(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public string CombinePath(string path1, string path2, string path3)
        {
            throw new NotImplementedException();
        }

        public void Copy(string from, string to)
        {
            throw new NotImplementedException();
        }

        public void CreateDirectory(string path)
        {
            Directories.Add(path);
        }

        public Stream CreateFileForWrite(string path)
        {
            var content = new VirtualFileContent();
            Files[path] = content;
            return content.GetWriteStream();
        }

        public Stream CreateNewFileForWrite(string path)
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string path)
        {
            return Directories.Contains(path);
        }

        public IEnumerable<Core.File> EnumerateFiles(string path, string searchMask = null)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            return Files.ContainsKey(path);
        }

        public string GetCurrentDirectory()
        {
            throw new NotImplementedException();
        }

        public IFilesystemDirectory GetDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public long GetFileSize(string path)
        {
            return Files[path].GetSize();
        }

        public void Move(string from, string to)
        {
            Files[to] = Files[from];
            Files.Remove(from);
        }

        public Stream OpenFileForRead(string path)
        {
            return Files[path].GetReadStream();
        }

        public void SetFile(string path, string content)
        {
            using (var writer = new StreamWriter(CreateFileForWrite(path)))
            {
                writer.Write(content);
            }
        }

        public string GetFileAsString(string path)
        {
            using (var reader = new StreamReader(OpenFileForRead(path)))
            {
                return reader.ReadToEnd();
            }
        }

        private Dictionary<string, VirtualFileContent> Files = new Dictionary<string, VirtualFileContent>();
        private HashSet<string> Directories = new HashSet<string>();
    }
}
