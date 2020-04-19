using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;
using Ctlg.Service.Utils;

namespace Ctlg.Service.FileStorage
{
    public class SimpleFileStorage : IFileStorage
    {

        public SimpleFileStorage(IFilesystemService filesystemService, HashCalculator hashCalculator, string backupRoot)
        {
            FilesystemService = filesystemService;
            HashCalculator = hashCalculator;

            FileStorageDirectory = FilesystemService.CombinePath(backupRoot, "file_storage");
            TempDirectory = FilesystemService.CombinePath(backupRoot, "temp");
        }

        private IFilesystemService FilesystemService { get; }
        private string FileStorageDirectory { get; }
        private string TempDirectory { get; }
        private HashCalculator HashCalculator { get; }

        public void AddFileFromStorage(File file, IFileStorage sourceStorage)
        {
            var previousHash = HashCalculator.GetExistingHashValue(file);
            file.Hashes.Clear();
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var tempFilePath = FilesystemService.CombinePath(TempDirectory, $"{timestamp}_{previousHash}");
            sourceStorage.CopyFileTo(previousHash.ToString(), tempFilePath);
            file.FullPath = tempFilePath;
            var calculatedHash = HashCalculator.CalculateHashForFile(file);
            if (previousHash != calculatedHash)
            {
                throw new Exception($"Caclulated hash does not match expected for file {file.Name}.");
            }
            var path = GetBackupPathForFile(file);
            PrepareDirectoryForFile(path);
            FilesystemService.Move(tempFilePath, path);
        }

        public void CopyFileTo(string hash, string destinationPath)
        {
            var storageFilePath = GetBackupFilePath(hash);
            if (!FilesystemService.FileExists(storageFilePath))
            {
                throw new Exception($"Could not restore {destinationPath}. Backup file {storageFilePath} not found.");
            }

            var destinationDir = FilesystemService.GetDirectoryName(destinationPath);
            FilesystemService.CreateDirectory(destinationDir);
            FilesystemService.Copy(storageFilePath, destinationPath);
        }

        public void AddFile(File file)
        {
            var path = GetBackupPathForFile(file);
            PrepareDirectoryForFile(path);
            FilesystemService.Copy(file.FullPath, path);
        }

        private void PrepareDirectoryForFile(string filePath)
        {
            var directoryPath = FilesystemService.GetDirectoryName(filePath);
            FilesystemService.CreateDirectory(directoryPath);
        }

        private string GetBackupFilePath(string hash)
        {
            var backupFileDir = FilesystemService.CombinePath(FileStorageDirectory, hash.Substring(0, 2));
            return FilesystemService.CombinePath(backupFileDir, hash);
        }

        public IEnumerable<byte[]> GetAllHashes()
        {
            var storageDir = FilesystemService.GetDirectory(FileStorageDirectory);
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
                        yield return FormatBytes.ToByteArray(file.Name);
                    }
                    else
                    {
                        DomainEvents.Raise(new Warning($"Unexpected file in storage: {file.Name}"));
                    }
                }
            }
        }

        public bool IsFileInStorage(File file)
        {
            var backupFile = GetBackupPathForFile(file);

            if (FilesystemService.FileExists(backupFile))
            {
                if (file.Size.HasValue && FilesystemService.GetFileSize(backupFile) != file.Size)
                {
                    throw new Exception($"The size of \"{file.FullPath}\" and \"{backupFile}\" do not match.");
                }

                return true;
            }

            return false;
        }

        private string GetBackupPathForFile(File file)
        {
            var hash = HashCalculator.GetExistingHashValue(file);

            return GetBackupFilePath(hash.ToString());
        }

        private Regex StorageSubDirRegex { get; } = new Regex("^[a-h0-9]{2}$", RegexOptions.IgnoreCase);
        private Regex StorageFileRegex { get; } = new Regex("^[a-h0-9]{64}$", RegexOptions.IgnoreCase);
    }
}
