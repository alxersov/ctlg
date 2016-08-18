using System;
using System.Collections.Generic;
using System.IO;
using File = Ctlg.Data.Model.File;


namespace Ctlg.Filesystem.Service
{
    public class FilesystemDirectory : IFilesystemDirectory
    {
        public File Directory { get; set; }

        public FilesystemDirectory(string path)
        {
            Initialize(new DirectoryInfo(path));
        }

        private FilesystemDirectory(DirectoryInfo directoryInfo)
        {
            Initialize(directoryInfo);
        }

        private void Initialize(DirectoryInfo directoryInfo)
        {
            _directoryInfo = directoryInfo;
            Directory = new File
            {
                IsDirectory = true,
                Name = directoryInfo.Name,
                FullPath = directoryInfo.FullName,
                FileCreatedDateTime = directoryInfo.CreationTimeUtc,
                FileModifiedDateTime = directoryInfo.LastWriteTimeUtc,
                RecordUpdatedDateTime = DateTime.UtcNow
            };
        }
        
        public IEnumerable<IFilesystemDirectory> EnumerateDirectories()
        {
            foreach (var dir in _directoryInfo.GetDirectories())
            {
                yield return new FilesystemDirectory(dir);
            }
        }
        public IEnumerable<File> EnumerateFiles()
        {
            foreach (var file in _directoryInfo.GetFiles())
            {
                yield return CreateFilesystemEntry(file);
            }
        }

        private DirectoryInfo _directoryInfo;

        protected static File CreateFilesystemEntry(FileInfo fileInfo)
        {
            return new File
            {
                FullPath = fileInfo.FullName,
                IsDirectory = false,
                Name = fileInfo.Name,
                FileCreatedDateTime = fileInfo.CreationTimeUtc,
                FileModifiedDateTime = fileInfo.LastWriteTimeUtc,
                Size = fileInfo.Length,
                RecordUpdatedDateTime = DateTime.UtcNow
            };
        }
    }
}
