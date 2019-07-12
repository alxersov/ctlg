using System.IO;

namespace Ctlg.Core.Interfaces
{
    public interface IArchiveService
    {
        bool IsArchiveExtension(string path);
        IArchive OpenArchive(Stream stream);
    }
}
