using System;
using System.Linq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Services
{
    public sealed class FileStorageService: IFileStorageService
    {
        public FileStorageService(IFilesystemService filesystemService)
        {
            FilesystemService = filesystemService;

            var currentDirectory = FilesystemService.GetCurrentDirectory();
            FileStorageDirectory = FilesystemService.CombinePath(currentDirectory, "file_storage");
        }

        public string GetBackupFilePath(string hash)
        {
            var backupFileDir = FilesystemService.CombinePath(FileStorageDirectory, hash.Substring(0, 2));
            return FilesystemService.CombinePath(backupFileDir, hash);
        }

        public void AddFileToStorage(File file)
        {
            var backupFile = GetBackupPathForFile(file);
            var backupFileDir = FilesystemService.GetDirectoryName(backupFile);
            FilesystemService.CreateDirectory(backupFileDir);
            FilesystemService.Copy(file.FullPath, backupFile);
        }

        public bool IsFileInStorage(File file)
        {
            var backupFile = GetBackupPathForFile(file);

            if (FilesystemService.FileExists(backupFile))
            {
                if (file.Size.HasValue && FilesystemService.GetFileSize(backupFile) != file.Size)
                {
                    throw new Exception($"The size of \"{file.RelativePath}\" and \"{backupFile}\" do not match.");
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

        public string FileStorageDirectory { get; }
        private IFilesystemService FilesystemService { get; }
    }
}
