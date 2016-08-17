using System.Collections.Generic;

namespace Ctlg.Filesystem.Service
{
    public interface IFilesystemDirectory: IFilesystemEntry
    {
        IEnumerable<IFilesystemDirectory> EnumerateDirectories();
        IEnumerable<IFilesystemEntry> EnumerateFiles();
    }
}
