﻿using System.IO;

namespace Ctlg.Filesystem.Service
{
    public class FilesystemService : FilesystemServiceBase, IFilesystemService
    {
        public IFilesystemDirectory GetDirectory(string path)
        {
            return new FilesystemDirectory(path);
        }

        public Stream OpenFileForRead(string path)
        {
            return File.OpenRead(path);
        }
    }
}
