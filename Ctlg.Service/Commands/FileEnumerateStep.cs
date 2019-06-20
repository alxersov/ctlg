using System;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;

namespace Ctlg.Service.Commands
{
    public class FileEnumerateStep: ITreeProvider
    {
        public FileEnumerateStep(IFilesystemService filesystemService)
        {
            FilesystemService = filesystemService;
        }

        public File ReadTree(string path)
        {
            var searchPattern = "*";

            var di = FilesystemService.GetDirectory(path);
            var root = ParseDirectory(di, searchPattern);
            root.Name = di.Directory.FullPath;

            return root;
        }

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
