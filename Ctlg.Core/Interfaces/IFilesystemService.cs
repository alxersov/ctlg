using System.IO;

namespace Ctlg.Core.Interfaces
{
    public interface IFilesystemService
    {
        IFilesystemDirectory GetDirectory(string path);
        Stream OpenFileForRead(string path);
        bool IsArchiveExtension(string path);
        IArchive OpenArchive(Stream stream);
    }
}
