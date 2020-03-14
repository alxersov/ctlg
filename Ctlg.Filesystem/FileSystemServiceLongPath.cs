using Ctlg.Core.Interfaces;
using Pri.LongPath;
using Stream = System.IO.Stream;
using FileMode = System.IO.FileMode;

namespace Ctlg.Filesystem
{
    public class FilesystemServiceLongPath : FilesystemServiceBase, IFilesystemService
    {
        public string CombinePath(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public string CombinePath(string path1, string path2, string path3)
        {
            return Path.Combine(path1, path2, path3);
        }

        public void Copy(string from, string to)
        {
            File.Copy(from, to, false);
        }

        public void Move(string from, string to)
        {
            File.Move(from, to);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public Stream CreateNewFileForWrite(string path)
        {
            return File.Open(path, FileMode.CreateNew);
        }

        public Stream CreateFileForWrite(string path)
        {
            return File.Open(path, FileMode.Create);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        public override IFilesystemDirectory GetDirectory(string path)
        {
            return new FilesystemDirectoryLongPath(path);
        }

        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public long GetFileSize(string path)
        {
            var fi = new FileInfo(path);

            return fi.Length;
        }

        public Stream OpenFileForRead(string path)
        {
            return File.OpenRead(path);
        }
    }
}
