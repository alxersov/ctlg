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

        public File ReadTree(string path, string searchPattern = null)
        {
            if (string.IsNullOrEmpty(searchPattern)) {
                searchPattern = "*";
            }

            var fsDirectory = FilesystemService.GetDirectory(path);
            ParseDirectory(fsDirectory, searchPattern);

            var root = fsDirectory.Directory;
            root.Name = root.FullPath;

            return root;
        }

        private void ParseDirectory(IFilesystemDirectory fsDirectory, string searchPattern)
        {
            var directory = fsDirectory.Directory;

            DomainEvents.Raise(new DirectoryFound(directory.RelativePath));

            foreach (var file in fsDirectory.EnumerateFiles(searchPattern))
            {
                AddNode(directory, file);

                DomainEvents.Raise(new FileFound(file.RelativePath));
            }

            foreach (var fsSubdir in fsDirectory.EnumerateDirectories())
            {
                AddNode(directory, fsSubdir.Directory);

                ParseDirectory(fsSubdir, searchPattern);
            }
        }

        private void AddNode(File directory, File node)
        {
            node.RelativePath = string.IsNullOrEmpty(directory.RelativePath) ?
                node.Name :
                FilesystemService.CombinePath(directory.RelativePath, node.Name);
            directory.Contents.Add(node);
        }

        protected IFilesystemService FilesystemService { get; }
    }
}
