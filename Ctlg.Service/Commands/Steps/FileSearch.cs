using System;
using System.Collections.Generic;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;

namespace Ctlg.Service.Commands.Steps
{
    public class FileSearch
    {
        public FileSearch(IFilesystemService filesystemService, string path, string searchPattern)
        {
            FilesystemService = filesystemService;
            Queue = InitializeQueue(path);
            SearchPattern = string.IsNullOrEmpty(searchPattern) ? "*" : searchPattern;
        }

        public IEnumerable<File> Run()
        {
            while (Queue.Count > 0)
            {
                DequeueDirectory();

                foreach (var file in CurrentDirectory.EnumerateFiles(SearchPattern))
                {
                    ProcessFile(file);

                    yield return file;
                }

                foreach (var subdir in CurrentDirectory.EnumerateDirectories())
                {
                    EnqueueDirectory(subdir);
                }
            }
        }

        private class DirectoryWithRelativePath
        {
            public DirectoryWithRelativePath(IFilesystemDirectory directory, string relativePath = null)
            {
                Directory = directory;
                RelativePath = relativePath;
            }

            public IFilesystemDirectory Directory { get; set; }
            public string RelativePath { get; set; }
        }

        private IFilesystemService FilesystemService { get; }
        private string SearchPattern { get; set; }
        private Queue<DirectoryWithRelativePath> Queue { get; set; }
        private string CurrentPath { get; set; }
        private IFilesystemDirectory CurrentDirectory { get; set; }

        private void DequeueDirectory()
        {
            var dirAndPath = Queue.Dequeue();
            CurrentDirectory = dirAndPath.Directory;
            CurrentPath = dirAndPath.RelativePath;
            if (!string.IsNullOrEmpty(CurrentPath))
            {
                DomainEvents.Raise(new DirectoryFound(CurrentPath));
            }
        }

        private void EnqueueDirectory(IFilesystemDirectory subdir)
        {
            var subdirPath = CombineRelativePath(CurrentPath, subdir.Name);
            Queue.Enqueue(new DirectoryWithRelativePath(subdir, subdirPath));
        }

        private void ProcessFile(File file)
        {
            file.RelativePath = CombineRelativePath(CurrentPath, file.Name);
            DomainEvents.Raise(new FileFound(file.RelativePath));
        }

        private string CombineRelativePath(string path1, string path2)
        {
            if (string.IsNullOrEmpty(path1))
            {
                return path2;
            }

            return FilesystemService.CombinePath(path1, path2);
        }

        private Queue<DirectoryWithRelativePath> InitializeQueue(string path)
        {
            var fsDirectory = FilesystemService.GetDirectory(path);

            var queue = new Queue<DirectoryWithRelativePath>();
            queue.Enqueue(new DirectoryWithRelativePath(fsDirectory));

            return queue;
        }
    }
}
