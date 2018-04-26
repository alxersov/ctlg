using System.IO;
using Ctlg.Core.Interfaces;

namespace Ctlg.Filesystem
{
    public class FilesystemService : FilesystemServiceBase, IFilesystemService
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

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public Stream CreateNewFileForWrite(string path)
        {
            return File.Open(path, FileMode.CreateNew);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public IFilesystemDirectory GetDirectory(string path)
        {
            return new FilesystemDirectory(path);
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
