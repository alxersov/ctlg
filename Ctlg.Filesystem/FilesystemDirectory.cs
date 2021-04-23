using System;
using System.Collections.Generic;
using System.IO;
using Ctlg.Core.Interfaces;
using File = Ctlg.Core.File;


namespace Ctlg.Filesystem
{
    public class FilesystemDirectory : IFilesystemDirectory
    {
        public FilesystemDirectory(string path) : this(new DirectoryInfo(path))
        {
        }

        public string Name
        {
            get
            {
                return _directoryInfo?.Name;
            }
        }

        private FilesystemDirectory(DirectoryInfo directoryInfo)
        {
            _directoryInfo = directoryInfo;
        }

        public IEnumerable<IFilesystemDirectory> EnumerateDirectories()
        {
            foreach (var directoryInfo in _directoryInfo.EnumerateDirectories())
            {
                yield return new FilesystemDirectory(directoryInfo);
            }
        }

        public IEnumerable<File> EnumerateFiles(string searchPattern)
        {
            foreach (var fileInfo in _directoryInfo.EnumerateFiles(searchPattern))
            {
                yield return CreateFilesystemEntry(fileInfo);
            }
        }

        private DirectoryInfo _directoryInfo;

        protected static File CreateFilesystemEntry(FileInfo fileInfo)
        {
            return new File
            {
                FullPath = fileInfo.FullName,
                Name = fileInfo.Name,
                FileModifiedDateTime = fileInfo.LastWriteTimeUtc,
                Size = fileInfo.Length
            };
        }
    }
}
