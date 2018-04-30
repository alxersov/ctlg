using System;
using Autofac.Features.Indexed;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;
using Ctlg.Service.Utils;
using StreamWriter = System.IO.StreamWriter;

namespace Ctlg.Service.Commands
{
    public class BackupCommand: TreeProcessingCommand, ICommand
    {
        public string Name { get; set; }

        private IHashFunction HashFunction;
        private StreamWriter FileListWriter;

        private IIndex<string, IHashFunction> HashFunctions { get; }
        private IDataService DataService { get; }
        private ICtlgService CtlgService { get; }

        public BackupCommand(IIndex<string, IHashFunction> hashFunctions,
                             IDataService dataService,
                             IFilesystemService filesystemService,
                             ICtlgService ctlgService) : base(filesystemService)
        {
            CtlgService = ctlgService;
            DataService = dataService;
            HashFunctions = hashFunctions;
        }

        public void Execute(ICtlgService ctlgService)
        {
            var hashFunctionName = "SHA-256";
            if (!HashFunctions.TryGetValue(hashFunctionName, out HashFunction))
            {
                throw new Exception($"Unsupported hash function {hashFunctionName}");
            }

            var root = ReadTree();

            using(var fileList = FilesystemService.CreateNewFileForWrite(Name))
            {
                using (FileListWriter = new StreamWriter(fileList))
                {
                    ProcessTree(root);
                }
            }
        }

        protected override void ProcessFile(File file)
        {
            try
            {
                var hash = CalculateHash(file.FullPath);
                var date = file.FileModifiedDateTime?.ToString("o");
                var path = file.RelativePath;

                var backupFile = CtlgService.GetBackupFilePath(hash);

                if (FilesystemService.FileExists(backupFile))
                {
                    if (FilesystemService.GetFileSize(backupFile) != file.Size)
                    {
                        throw new Exception($"The size of \"{path}\" and \"{backupFile}\" do not match.");
                    }
                }
                else
                {
                    var backupFileDir = FilesystemService.GetDirectoryName(backupFile);
                    FilesystemService.CreateDirectory(backupFileDir);
                    FilesystemService.Copy(file.FullPath, backupFile);
                }

                var fileListEntry = $"{hash} {date} {file.Size} {path}";
                FileListWriter.WriteLine(fileListEntry);

                DomainEvents.Raise(new BackupEntryCreated(fileListEntry));
            }
            catch (Exception e)
            {
                DomainEvents.Raise(new ExceptionEvent(e));
            }
        }

        private string CalculateHash(string path)
        {
            using (var stream = FilesystemService.OpenFileForRead(path))
            {
                var hash = HashFunction.CalculateHash(stream);
                return FormatBytes.ToHexString(hash.Value);
            }
        }
    }
}
