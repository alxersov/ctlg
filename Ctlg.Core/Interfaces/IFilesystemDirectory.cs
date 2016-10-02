using System.Collections.Generic;

namespace Ctlg.Core.Interfaces
{
    public interface IFilesystemDirectory
    {
        File Directory { get; set; }

        IEnumerable<IFilesystemDirectory> EnumerateDirectories();
        IEnumerable<File> EnumerateFiles(string searchPattern);
    }
}
