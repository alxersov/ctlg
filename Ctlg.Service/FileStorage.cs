using System;
using System.Linq;
using System.Text.RegularExpressions;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;
using Ctlg.Service.Utils;

namespace Ctlg.Service
{
    public class FileStorage: IFileStorage
    {

        public FileStorage(IFilesystemService filesystemService, ICtlgService ctlgService, IFileStorageIndexService indexService,
            string backupRoot, bool shouldUseIndex, bool shouldExistingHashMatchCaclulated)
        {
            BackupRoot = backupRoot;
            FilesystemService = filesystemService;
            CtlgService = ctlgService;
            IndexService = indexService;
            BackupRootDirectory = backupRoot;
            HashFunction = ctlgService.GetHashFunction("SHA-256");
            ShouldUseIndex = shouldUseIndex;

            ShouldExistingHashMatchCaclulated = shouldExistingHashMatchCaclulated;
            FileStorageDirectory = FilesystemService.CombinePath(backupRoot, "file_storage");
        }

        public IFilesystemService FilesystemService { get; }
        public ICtlgService CtlgService { get; }
        public IFileStorageIndexService IndexService { get; }
        private IFileStorageIndex _index;
        public string BackupRoot { get; }
        public string FileStorageDirectory { get; }
        public string BackupRootDirectory { get; }
        public IHashFunction HashFunction { get; }
        public bool ShouldUseIndex { get; }
        public bool ShouldExistingHashMatchCaclulated { get; }

        public BackupFileStatus AddFileToStorage(File file)
        {
            var fileStatus = FindFile(file);
            if (fileStatus.IsNotFound())
            {
                if (fileStatus.HasFlag(BackupFileStatus.HashRecalculated))
                {
                    CopyFileToStorage(file);

                    var hash = file.Hashes.First(h => h.HashAlgorithmId == (int)HashAlgorithmId.SHA256);
                    Index.Add(hash.Value);
                }
                else
                {
                    var previousHash = file.Hashes.First(h => h.HashAlgorithmId == (int)HashAlgorithmId.SHA256);
                    file.Hashes.Clear();
                    fileStatus = AddFileToStorage(file);

                    if (ShouldExistingHashMatchCaclulated)
                    {
                        var calculatedHash = file.Hashes.First(h => h.HashAlgorithmId == (int)HashAlgorithmId.SHA256);

                        if (previousHash != calculatedHash)
                        {
                            throw new Exception($"Caclulated hash does not match expected for file {file.FullPath}.");
                        }
                    }
                }
            }

            return fileStatus;
        }

        private void CopyFileToStorage(File file)
        {
            var backupFile = GetBackupPathForFile(file);
            var backupFileDir = FilesystemService.GetDirectoryName(backupFile);
            FilesystemService.CreateDirectory(backupFileDir);
            FilesystemService.Copy(file.FullPath, backupFile);
        }

        public string GetBackupFilePath(string hash)
        {
            var backupFileDir = FilesystemService.CombinePath(FileStorageDirectory, hash.Substring(0, 2));
            return FilesystemService.CombinePath(backupFileDir, hash);
        }

        public void RebuildIndex()
        {
            _index = IndexService.GetIndex(BackupRootDirectory);
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
                        var hash = FormatBytes.ToByteArray(file.Name);
                        Index.Add(hash);
                    }
                    else
                    {
                        DomainEvents.Raise(new Warning($"Unexpected file in storage: {file.Name}"));
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_index != null)
            {
                _index.Save();
                _index = null;
            }
        }

        private IFileStorageIndex Index
        {
            get
            {
                if (_index == null)
                {
                    _index = IndexService.GetIndex(BackupRoot);
                    _index.Load();
                }

                return _index;
            }
        }

        private bool IsFileInStorage(File file)
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
            var hash = file.Hashes.First(h => h.HashAlgorithmId == (int)HashAlgorithmId.SHA256);

            return GetBackupFilePath(hash.ToString());
        }

        private Hash GetHashForFile(File file, out bool hashCalculated)
        {
            var hash = GetExistingHashValue(file);
            if (hash != null)
            {
                hashCalculated = false;
                return hash;
            }
            else
            {
                hashCalculated = true;
                return CtlgService.CalculateHashForFile(file, HashFunction);
            }
        }

        private BackupFileStatus FindFile(File file)
        {
            var status = default(BackupFileStatus);

            var hash = GetHashForFile(file, out bool isHashCalculated);
            if (isHashCalculated)
            {
                status |= BackupFileStatus.HashRecalculated;
            }

            if (ShouldUseIndex && Index.IsInIndex(hash.Value))
            {
                status |= BackupFileStatus.FoundInIndex;
            }
            else if (IsFileInStorage(file))
            {
                status |= BackupFileStatus.FoundInStorage;
            }

            return status;
        }

        private static Hash GetExistingHashValue(File file)
        {
            return file.Hashes.FirstOrDefault(h => h.HashAlgorithmId == (int)HashAlgorithmId.SHA256);
        }

        private Regex StorageSubDirRegex { get; } = new Regex("^[a-h0-9]{2}$", RegexOptions.IgnoreCase);
        private Regex StorageFileRegex { get; } = new Regex("^[a-h0-9]{64}$", RegexOptions.IgnoreCase);
    }
}
