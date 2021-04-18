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

        public string HashAlgorithmName => HashCalculator.Name;

        public byte[] AddFileFromStorage(SnapshotRecord snapshotRecord, IFileStorage sourceStorage)
        {
            var previousHash = snapshotRecord.Hash;

            var tempFilePath = GetTempFilePath(previousHash);
            sourceStorage.CopyFileTo(snapshotRecord, tempFilePath);

            var calculatedHash = HashCalculator.CalculateHashForFile(tempFilePath);
            if (sourceStorage.HashAlgorithmName == HashAlgorithmName && !ByteArrayComparer.AreEqual(previousHash, calculatedHash))
            {
                throw new Exception($"Caclulated hash does not match expected for file {snapshotRecord.RelativePath}.");
            }
            var path = GetBackupFilePath(calculatedHash);
            PrepareDirectoryForFile(path);
            FilesystemService.Move(tempFilePath, path);
            return calculatedHash;
        }

        public void CopyFileTo(SnapshotRecord snapshotRecord, string destinationPath)
        {
            CopyFileTo(FormatBytes.ToHexString(snapshotRecord.Hash), destinationPath);
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

        public void AddFile(File file, byte[] hash)
        {
            var path = GetBackupFilePath(hash);
            PrepareDirectoryForFile(path);
            FilesystemService.Copy(file.FullPath, path);
        }

        private void PrepareDirectoryForFile(string filePath)
        {
            var directoryPath = FilesystemService.GetDirectoryName(filePath);
            FilesystemService.CreateDirectory(directoryPath);
        }

        private string GetBackupFilePath(byte[] hash)
        {
            return GetBackupFilePath(FormatBytes.ToHexString(hash));
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

                DomainEvents.Raise(new EnumeratingHashes(dir.Directory.Name));

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

        public bool IsFileInStorage(SnapshotRecord snapshotRecord)
        {
            var backupFile = GetBackupFilePath(snapshotRecord.Hash);

            if (FilesystemService.FileExists(backupFile))
            {
                if (FilesystemService.GetFileSize(backupFile) != snapshotRecord.Size)
                {
                    throw new Exception($"The size of \"{snapshotRecord.RelativePath}\" and \"{backupFile}\" do not match.");
                }

                return true;
            }

            return false;
        }

        public bool IsFileInStorage(File file, byte[] hash)
        {
            var backupFile = GetBackupFilePath(hash);

            if (FilesystemService.FileExists(backupFile))
            {
                if (file.Size.HasValue && FilesystemService.GetFileSize(backupFile) != file.Size)
                {
                    throw new Exception($"The size of \"{file.FullPath ?? file.Name}\" and \"{backupFile}\" do not match.");
                }

                return true;
            }

            return false;
        }

        private string GetTempFilePath(byte[] previousHash)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var fileName = $"{timestamp}_{FormatBytes.ToHexString(previousHash)}";

            return FilesystemService.CombinePath(TempDirectory, fileName);
        }

        public bool VerifyFileByHash(byte[] hash)
        {
            var expectedHashString = FormatBytes.ToHexString(hash);
            var path = GetBackupFilePath(expectedHashString);
            var calculatedHash = HashCalculator.CalculateHashForFile(path);
            var calculatedHashString = FormatBytes.ToHexString(calculatedHash);

            if (expectedHashString != calculatedHashString)
            {
                throw new Exception($"Hash mismatch for {path} expected: {expectedHashString} got {calculatedHashString}");
            }

            return true;

        }

        private Regex StorageSubDirRegex { get; } = new Regex("^[a-h0-9]{2}$", RegexOptions.IgnoreCase);
        private Regex StorageFileRegex { get; } = new Regex("^[a-h0-9]{64}$", RegexOptions.IgnoreCase);
    }
}
