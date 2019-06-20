using System;
using StreamReader = System.IO.StreamReader;
using Ctlg.Core;
using Ctlg.Service.Events;
using Ctlg.Service.Utils;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Commands
{
    public class SnapshotReader : ISnapshotReader
    {
        public SnapshotReader(ICtlgService ctlgService, IFilesystemService filesystemService)
        {
            CtlgService = ctlgService;
            FilesystemService = filesystemService;
        }

        public ICtlgService CtlgService { get; }
        public IFilesystemService FilesystemService { get; }

        public void ReadHashesFromLatestSnapshot(string snapshotName, File destinationTree)
        {
            var snapshotFileName = CtlgService.GetLastSnapshotFile(snapshotName);
            if (snapshotFileName == null)
            {
                return;
            }

            var backupDirectory = CtlgService.GetBackupSnapshotDirectory(snapshotName);
            var snapshotPath = FilesystemService.CombinePath(backupDirectory, snapshotFileName);

            CtlgService.SortTree(destinationTree);

            using (var stream = FilesystemService.OpenFileForRead(snapshotPath))
            {
                using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                {
                    var line = reader.ReadLine();
                    while (line != null)
                    {
                        try
                        {
                            ProcessBackupLine(line, destinationTree);
                        }
                        catch (Exception ex)
                        {
                            DomainEvents.Raise(new ErrorEvent(ex));
                        }

                        line = reader.ReadLine();
                    }
                }
            }
        }

        private void ProcessBackupLine(string line, Core.File root)
        {
            var record = new SnapshotRecord(line);
            var path = record.Name.Split('\\', '/');

            var i = 0;
            File currentFile = root;
            while (i < path.Length && currentFile != null)
            {
                currentFile = CtlgService.GetInnerFile(currentFile, path[i]);
                ++i;
            }

            if (i == path.Length &&
                currentFile.Size == record.Size &&
                currentFile.FileModifiedDateTime == record.Date)
            {
                currentFile.Hashes.Add(new Hash(HashAlgorithmId.SHA256, FormatBytes.ToByteArray(record.Hash)));
            }
        }
    }
}
