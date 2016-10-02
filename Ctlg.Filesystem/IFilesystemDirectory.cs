using System.Collections.Generic;
using Ctlg.Core;

namespace Ctlg.Filesystem
{
    public interface IFilesystemDirectory
    {
        File Directory { get; set; }

        IEnumerable<IFilesystemDirectory> EnumerateDirectories();
        IEnumerable<File> EnumerateFiles(string searchPattern);
    }
}
