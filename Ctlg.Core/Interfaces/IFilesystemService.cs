using System.Collections.Generic;
using System.IO;

namespace Ctlg.Core.Interfaces
{
    public interface IFilesystemService
    {
        IFilesystemDirectory GetDirectory(string path);
        IEnumerable<File> EnumerateFiles(string path, string searchMask = null);
        Stream OpenFileForRead(string path);
        Stream CreateNewFileForWrite(string path);
        Stream CreateFileForWrite(string path);
        void CreateDirectory(string path);
        string GetCurrentDirectory();
        bool FileExists(string path);
        bool DirectoryExists(string path);
        long GetFileSize(string path);
        void Copy(string from, string to);
        void Move(string from, string to);
        string CombinePath(string path1, string path2);
        string CombinePath(string path1, string path2, string path3);
        string GetDirectoryName(string path);
    }
}
