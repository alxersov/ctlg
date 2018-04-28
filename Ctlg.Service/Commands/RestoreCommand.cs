using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
        private static Regex BackupLineRegex = new Regex(@"^(?<hash>[a-h0-9]{64})\s(?<date>[0-9:.TZ-]{19,28})\s(?<size>[0-9]{1,10})\s(?<name>\S.*)$", RegexOptions.IgnoreCase);


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
                            DomainEvents.Raise(new ExceptionEvent(ex));
                        }

                        line = reader.ReadLine();
                    }
                }
            }
        }

        private void ProcessBackupLine(string line)
        {
            var match = BackupLineRegex.Match(line);

            if (!match.Success)
            {
                throw new Exception($"Unexpected list line {line}.");
            }

            var hash = match.Groups["hash"].Value;
            var size = long.Parse(match.Groups["size"].Value);
            var date = DateTime.Parse(match.Groups["date"].Value);
            var name = match.Groups["name"].Value;

            var backupFilePath = CtlgService.GetBackupFilePath(hash);
            if (!FileSystemService.FileExists(backupFilePath))
            {
                throw new Exception($"Could not restore {name}. Backup file {backupFilePath} not found.");
            }

            var destinationFile = FileSystemService.CombinePath(Path, name);
            var destinationDir = FileSystemService.GetDirectoryName(destinationFile);
            FileSystemService.CreateDirectory(destinationDir);
            FileSystemService.Copy(backupFilePath, destinationFile);

            DomainEvents.Raise(new BackupEntryProcessed(name));
        }
    }
}
