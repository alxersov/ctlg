using System;
using System.IO;
using System.Linq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;
using Ctlg.Service.Utils;
using File = Ctlg.Core.File;

namespace Ctlg.Service.Commands
{
    public class SnapshotWriter: ISnapshotWriter
    {
        public SnapshotWriter(StreamWriter writer, IFilesystemService filesystemService, ICtlgService ctlgService, IHashFunction hashFunction)
        {
            _streamWriter = writer;
            FilesystemService = filesystemService;
            CtlgService = ctlgService;
            HashFunction = hashFunction;
        }

        public void AddFile(File file)
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

                var fileListEntry = new SnapshotRecord(hash, file.FileModifiedDateTime ?? DateTime.MinValue, file.Size ?? 0, path);
                _streamWriter.WriteLine(fileListEntry);

                DomainEvents.Raise(new BackupEntryCreated(fileListEntry, hashCalculated, newFileAddedToStorage));
            }
            catch (Exception e)
            {
                DomainEvents.Raise(new ErrorEvent(e));
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

        public void Dispose()
        {
            _streamWriter.Dispose();
        }

        private StreamWriter _streamWriter;

        public IFilesystemService FilesystemService { get; }
        public ICtlgService CtlgService { get; }
        public IHashFunction HashFunction { get; }
    }
}
