﻿using System;
using System.Collections.Generic;
using System.IO;
using Ctlg.Core.Interfaces;
using File = Ctlg.Core.File;


namespace Ctlg.Filesystem
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
            foreach (var directoryInfo in _directoryInfo.EnumerateDirectories())
            {
                var dir = new FilesystemDirectory(directoryInfo);
                dir.Directory.RelativePath = CombineRelativePath(directoryInfo.Name);

                yield return dir;
            }
        }

        public IEnumerable<File> EnumerateFiles(string searchPattern)
        {
            foreach (var fileInfo in _directoryInfo.EnumerateFiles(searchPattern))
            {
                var file = CreateFilesystemEntry(fileInfo);
                file.RelativePath = CombineRelativePath(fileInfo.Name);

                yield return file;
            }
        }

        private DirectoryInfo _directoryInfo;

        private string CombineRelativePath(string name)
        {
            return Directory.IsRoot ?
                name :
                Path.Combine(Directory.RelativePath, name);
        }

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
