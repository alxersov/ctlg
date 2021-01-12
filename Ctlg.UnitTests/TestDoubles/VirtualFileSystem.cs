using System;
using System.IO;
using Ctlg.Core.Interfaces;
using Ctlg.Filesystem;

namespace Ctlg.UnitTests.TestDoubles
{
    public class VirtualFileSystem : FilesystemServiceBase, IFilesystemService
    {
        public const string Separator = "/";

        public string CurrentDirectory { get; set; } = "home";

        public VirtualFileSystem()
        {
            Root = new VirtualFilesystemNode("", "");

            CreateDirectory(CurrentDirectory);
        }

        public string CombinePath(string path1, string path2)
        {
            return $"{path1}{Separator}{path2}";
        }

        public string CombinePath(string path1, string path2, string path3)
        {
            return CombinePath(CombinePath(path1, path2), path3);
        }

        public void Copy(string from, string to)
        {
            var directoryFromPath = GetDirectoryName(from);
            var fileFromName = GetFileName(from);
            var fromDirectory = GetVirtualDirectory(directoryFromPath);

            var original = fromDirectory.Files[fileFromName];

            using (var output = CreateFileForWrite(to))
            {
                original.GetReadStream().CopyTo(output);
            }
        }

        public void CreateDirectory(string path)
        {
            var names = SplitPath(path);

            var dir = Root;
            foreach (var name in names)
            {
                dir = dir.CreateDirectory(name);
            }
        }

        public VirtualFilesystemNode GetVirtualDirectory(string path)
        {
            var names = SplitPath(path);

            var dir = Root;
            foreach (var name in names)
            {
                dir = dir.GetDirectory(name);
                if (dir == null)
                {
                    return null;
                }
            }

            return dir;
        }

        public Stream CreateFileForWrite(string path)
        {
            var content = new VirtualFileContent();
            SetFileContent(path, content);
            return content.GetWriteStream();
        }

        public Stream CreateNewFileForWrite(string path)
        {
            return CreateFileForWrite(path);
        }

        public bool DirectoryExists(string path)
        {
            return GetVirtualDirectory(path) != null;
        }

        public bool FileExists(string path)
        {
            return GetFileContent(path) != null;
        }

        public string GetCurrentDirectory()
        {
            return CurrentDirectory;
        }

        public override IFilesystemDirectory GetDirectory(string path)
        {
            return new VirtualDirectory(GetVirtualDirectory(path));
        }

        public string GetDirectoryName(string path)
        {
            var index = path.LastIndexOf(Separator);

            return index < 0 ? string.Empty : path.Substring(0, index);
        }

        public string GetFileName(string path)
        {
            var index = path.LastIndexOf(Separator);

            return index < 0 ? path : path.Substring(index + 1);
        }

        public long GetFileSize(string path)
        {
            return GetFileContent(path).GetSize();
        }

        public void Move(string from, string to)
        {
            var content = GetFileContent(from);
            RemoveFile(from);
            SetFileContent(to, content);
        }

        public Stream OpenFileForRead(string path)
        {
            var content = GetFileContent(path);

            if (content == null)
            {
                throw new FileNotFoundException();
            }

            return content.GetReadStream();
        }

        public Stream OpenFileForWrite(string path)
        {
            var content = GetFileContent(path);

            var srcStream = content.GetReadStream();
            var dstStream = content.GetWriteStream();

            srcStream.CopyTo(dstStream);

            return dstStream;
        }

        public void SetFile(string path, string content, DateTime modifiedTime = default)
        {
            CreateDirectory(GetDirectoryName(path));
            using (var writer = new StreamWriter(CreateFileForWrite(path)))
            {
                writer.Write(content);
            }

            GetFileContent(path).FileModifiedDateTime = modifiedTime;
        }

        public void RemoveFile(string path)
        {
            var directory = GetVirtualDirectory(GetDirectoryName(path));
            var fileName = GetFileName(path);
            directory.Files.Remove(fileName);
        }

        public string GetFileAsString(string path)
        {
            using (var reader = new StreamReader(OpenFileForRead(path)))
            {
                return reader.ReadToEnd();
            }
        }

        private string[] SplitPath(string path)
        {
            return path.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
        }

        private VirtualFileContent GetFileContent(string path)
        {
            var directory = GetVirtualDirectory(GetDirectoryName(path));
            var fileName = GetFileName(path);
            return directory != null && directory.Files.ContainsKey(fileName) ? directory.Files[fileName] : null;
        }

        private void SetFileContent(string path, VirtualFileContent content)
        {
            var directory = GetVirtualDirectory(GetDirectoryName(path));
            var fileName = GetFileName(path);
            directory.Files[fileName] = content;
        }

        private VirtualFilesystemNode Root { get; }
    }
}
