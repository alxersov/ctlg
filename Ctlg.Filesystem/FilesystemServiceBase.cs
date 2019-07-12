using System;
using System.Collections.Generic;
using Ctlg.Core.Interfaces;

namespace Ctlg.Filesystem
{
    public abstract class FilesystemServiceBase
    {
        public IEnumerable<Core.File> EnumerateFiles(string path, string searchMask = null)
        {
            var dir = GetDirectory(path);
            return dir.EnumerateFiles(searchMask ?? "*");
        }

        public abstract IFilesystemDirectory GetDirectory(string path);
    }
}
