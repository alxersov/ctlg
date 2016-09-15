using System.Collections.Generic;
using Ctlg.Data.Model;

namespace Ctlg.Filesystem.Service
{
    public interface IFilesystemDirectory
    {
        File Directory { get; set; }

        IEnumerable<IFilesystemDirectory> EnumerateDirectories();
        IEnumerable<File> EnumerateFiles(string searchPattern);
    }
}
