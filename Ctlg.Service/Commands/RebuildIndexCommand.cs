using System;
using System.Text.RegularExpressions;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;
using Ctlg.Service.Utils;

namespace Ctlg.Service.Commands
{
    public class RebuildIndexCommand : ICommand
    {
        public RebuildIndexCommand(IFileStorageService fileStorageService, IFilesystemService filesystemService, IIndexService indexService, IIndexFileService indexFileService)
        {
            FileStorageService = fileStorageService;
            FilesystemService = filesystemService;
            IndexService = indexService;
            IndexFileService = indexFileService;
        }

        public void Execute()
        {
            var storageDir = FilesystemService.GetDirectory(FileStorageService.FileStorageDirectory);
            foreach (var dir in storageDir.EnumerateDirectories())
            {
                if (!StorageSubDirRegex.IsMatch(dir.Directory.Name))
                {
                    DomainEvents.Raise(new Warning($"Unexpected directory in storage: {dir.Directory.Name}"));
                    continue;
                }

                foreach (var file in dir.EnumerateFiles("*"))
                {
                    if (StorageFileRegex.IsMatch(file.Name))
                    {
                        var hash = FormatBytes.ToByteArray(file.Name);
                        IndexService.Add(hash);
                    }
                    else
                    {
                        DomainEvents.Raise(new Warning($"Unexpected file in storage: {file.Name}"));
                    }
                }
            }
            IndexFileService.Save();
        }

        private IFileStorageService FileStorageService { get; }
        private IFilesystemService FilesystemService { get; }
        private IIndexService IndexService { get; }
        private IIndexFileService IndexFileService { get; }
        private Regex StorageSubDirRegex { get; } = new Regex("^[a-h0-9]{2}$", RegexOptions.IgnoreCase);
        private Regex StorageFileRegex { get; } = new Regex("^[a-h0-9]{64}$", RegexOptions.IgnoreCase);
    }
}
