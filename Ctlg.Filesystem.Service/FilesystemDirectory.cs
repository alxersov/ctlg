using System;
using System.Collections.Generic;
using System.IO;
using File = Ctlg.Data.Model.File;


namespace Ctlg.Filesystem.Service
{
    public class FilesystemDirectory : FilesystemEntry, IFilesystemDirectory
    {
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
            FullPath = directoryInfo.FullName;
            File = new File
            {
                IsDirectory = true,
                Name = directoryInfo.Name,
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
        public IEnumerable<IFilesystemEntry> EnumerateFiles()
        {
            foreach (var file in _directoryInfo.GetFiles())
            {
                yield return CreateFilesystemEntry(file);
            }
        }

        private DirectoryInfo _directoryInfo;

        protected static FilesystemEntry CreateFilesystemEntry(FileInfo fileInfo)
        {
            return new FilesystemEntry
            {
                FullPath = fileInfo.FullName,
                File = new File
                {
                    IsDirectory = false,
                    Name = fileInfo.Name,
                    FileCreatedDateTime = fileInfo.CreationTimeUtc,
                    FileModifiedDateTime = fileInfo.LastWriteTimeUtc,
                    Size = fileInfo.Length,
                    RecordUpdatedDateTime = DateTime.UtcNow
                }
            };
        }
    }
}
