using System;
using Autofac.Features.Indexed;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;
using Ctlg.Service.Utils;
using StreamWriter = System.IO.StreamWriter;
using StreamReader = System.IO.StreamReader;
using System.Linq;

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

            FindFilesInPreviousSnapshot(root);

            var backupDirectory = CtlgService.GetBackupSnapshotDirectory(Name);
            FilesystemService.CreateDirectory(backupDirectory);

            var snapshotName = CtlgService.GenerateSnapshotFileName();
            var snapshotPath = FilesystemService.CombinePath(backupDirectory, snapshotName);

            DomainEvents.Raise(new BackupCommandStarted(snapshotPath, CtlgService.FileStorageDirectory));

            using (var fileList = FilesystemService.CreateNewFileForWrite(snapshotPath))
            {
                using (FileListWriter = new StreamWriter(fileList))
                {
                    ProcessTree(root);
                }
            }
        }

        private void FindFilesInPreviousSnapshot(File root)
        {
            CtlgService.SortTree(root);

            var snapshotName = CtlgService.GetLastSnapshotFile(Name);
            if (snapshotName == null)
            {
                return;
            }

            var backupDirectory = CtlgService.GetBackupSnapshotDirectory(Name);
            var snapshotPath = FilesystemService.CombinePath(backupDirectory, snapshotName);

            using (var stream = FilesystemService.OpenFileForRead(snapshotPath))
            {
                using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                {
                    var line = reader.ReadLine();
                    while (line != null)
                    {
                        try
                        {
                            ProcessBackupLine(line, root);
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

        protected override void ProcessFile(File file)
        {
            try
            {
                var hashCalculated = false;
                var newFileAddedToStorage = false;

                var hash = GetExistingHashValue(file);
                if (hash == null)
                {
                    hash = CalculateHash(file.FullPath);
                    hashCalculated = true;
                }

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
                    newFileAddedToStorage = true;
                }

                var fileListEntry = $"{hash} {date} {file.Size} {path}";
                FileListWriter.WriteLine(fileListEntry);

                DomainEvents.Raise(new BackupEntryCreated(fileListEntry, hashCalculated, newFileAddedToStorage));
            }
            catch (Exception e)
            {
                DomainEvents.Raise(new ExceptionEvent(e));
            }
        }

        private static string GetExistingHashValue(File file)
        {
            var knownHash = file.Hashes.FirstOrDefault(h => h.HashAlgorithmId == (int)HashAlgorithmId.SHA256);
            if (knownHash != null)
            {
                return FormatBytes.ToHexString(knownHash.Value);
            }

            return null;
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
