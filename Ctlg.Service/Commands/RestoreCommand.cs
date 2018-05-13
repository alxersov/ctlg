using System;
using System.IO;
using System.Text;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;

namespace Ctlg.Service.Commands
{
    public class RestoreCommand: ICommand
    {
        public string Name { get; set; }
        public string Path { get; set; }

        private IFilesystemService FileSystemService { get; }
        private ICtlgService CtlgService { get; }

        public RestoreCommand(IFilesystemService fileSystemService, ICtlgService ctlgService)
        {
            CtlgService = ctlgService;
            FileSystemService = fileSystemService;
        }

        public void Execute(ICtlgService ctlgService)
        {
            using (var stream = FileSystemService.OpenFileForRead(Name))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var line = reader.ReadLine();
                    while (line != null)
                    {
                        try
                        {
                            ProcessBackupLine(line);
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

        private void ProcessBackupLine(string line)
        {
            var record = new SnapshotRecord(line);

            var backupFilePath = CtlgService.GetBackupFilePath(record.Hash);
            if (!FileSystemService.FileExists(backupFilePath))
            {
                throw new Exception($"Could not restore {record.Name}. Backup file {backupFilePath} not found.");
            }

            var destinationFile = FileSystemService.CombinePath(Path, record.Name);
            var destinationDir = FileSystemService.GetDirectoryName(destinationFile);
            FileSystemService.CreateDirectory(destinationDir);
            FileSystemService.Copy(backupFilePath, destinationFile);

            DomainEvents.Raise(new BackupEntryRestored(record.Name));
        }
    }
}
