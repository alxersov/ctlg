using System.Collections.Generic;

namespace Ctlg.Core.Interfaces
{
    public interface IFilesystemDirectory
    {
        string Name { get; }

        IEnumerable<IFilesystemDirectory> EnumerateDirectories();
        IEnumerable<File> EnumerateFiles(string searchPattern);
    }
}
