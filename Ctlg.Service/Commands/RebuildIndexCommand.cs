using System;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Utils;

namespace Ctlg.Service.Commands
{
    public class RebuildIndexCommand : ICommand
    {
        public RebuildIndexCommand(ICtlgService ctlgService, IFilesystemService filesystemService, IIndexService indexService, IIndexFileService indexFileService)
        {
            CtlgService = ctlgService;
            FilesystemService = filesystemService;
            IndexService = indexService;
            IndexFileService = indexFileService;
        }


        public void Execute()
        {
            var storageDir = FilesystemService.GetDirectory(CtlgService.FileStorageDirectory);
            foreach (var dir in storageDir.EnumerateDirectories())
            {
                foreach (var file in dir.EnumerateFiles("*"))
                {
                    var hash = FormatBytes.ToByteArray(file.Name);
                    IndexService.Add(hash);
                }
            }
            IndexFileService.Save();
        }

        private ICtlgService CtlgService { get; }
        private IFilesystemService FilesystemService { get; }
        private IIndexService IndexService { get; }
        private IIndexFileService IndexFileService { get; }
    }
}
