using System;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;

namespace Ctlg.Service.Commands
{
    public abstract class TreeProcessingCommand
    {
        public string Path { get; set; }
        public string SearchPattern { get; set; }

        public TreeProcessingCommand(IFilesystemService filesystemService)
        {
            FilesystemService = filesystemService;
        }

        public File ReadTree()
        {
            var searchPattern = string.IsNullOrEmpty(SearchPattern) ? "*" :
                                      SearchPattern;

            var di = FilesystemService.GetDirectory(Path);
            var root = ParseDirectory(di, searchPattern);
            root.Name = di.Directory.FullPath;

            return root;
        }

        public void ProcessTree(File directory)
        {
            foreach (var file in directory.Contents)
            {
                if (file.IsDirectory)
                {
                    ProcessTree(file);
                }
                else
                {
                    ProcessFile(file);
                }
            }
        }

        protected abstract void ProcessFile(File file);

        private File ParseDirectory(IFilesystemDirectory fsDirectory, string searchPattern)
        {
            var directory = fsDirectory.Directory;

            DomainEvents.Raise(new DirectoryFound(directory.RelativePath));

            foreach (var file in fsDirectory.EnumerateFiles(searchPattern))
            {
                DomainEvents.Raise(new FileFound(file.RelativePath));

                directory.Contents.Add(file);
            }

            foreach (var dir in fsDirectory.EnumerateDirectories())
            {
                directory.Contents.Add(ParseDirectory(dir, searchPattern));
            }

            return directory;
        }


        protected IFilesystemService FilesystemService { get; }
    }
}
