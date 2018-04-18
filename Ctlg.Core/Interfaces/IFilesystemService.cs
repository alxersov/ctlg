using System.IO;

namespace Ctlg.Core.Interfaces
{
    public interface IFilesystemService
    {
        IFilesystemDirectory GetDirectory(string path);
        Stream OpenFileForRead(string path);
        Stream CreateNewFileForWrite(string path);
        void CreateDirectory(string path);
        bool FileExists(string path);
        long GetFileSize(string path);
        void Copy(string from, string to);
        string CombinePath(string path1, string path2);
        string CombinePath(string path1, string path2, string path3);
        bool IsArchiveExtension(string path);
        IArchive OpenArchive(Stream stream);
    }
}
