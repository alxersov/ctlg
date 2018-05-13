using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.Indexed;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Commands;
using Ctlg.Service.Events;
using File = Ctlg.Core.File;

namespace Ctlg.Service
{
    public class CtlgService : ICtlgService
    {
        public CtlgService(IDataService dataService, IFilesystemService filesystemService, IIndex<string, IHashFunction> hashFunction)
        {
            DataService = dataService;
            FilesystemService = filesystemService;
            HashFunctions = hashFunction;

            CurrentDirectory = FilesystemService.GetCurrentDirectory();
            SnapshotsDirectory = FilesystemService.CombinePath(CurrentDirectory, "snapshots");
            FileStorageDirectory = FilesystemService.CombinePath(CurrentDirectory, "file_storage");
        }

        public string CurrentDirectory { get; private set; }
        public string SnapshotsDirectory { get; private set; }
        public string FileStorageDirectory {get; private set; }

        public void ApplyDbMigrations()
        {
            DataService.ApplyDbMigrations();
        }

        public void ListFiles()
        {
            OutputFiles(DataService.GetFiles());
        }

        public void FindFiles(Hash hash, long? size, string namePattern)
        {
            var files = DataService.GetFiles(hash, size, namePattern);

            foreach (var f in files)
            {
                DomainEvents.Raise(new FileFoundInDb(f));
            }
        }

        public void Show(int catalgoEntryId)
        {
            var entry = DataService.GetCatalogEntry(catalgoEntryId);

            if (entry == null)
            {
                DomainEvents.Raise(new CatalogEntryNotFound(catalgoEntryId));
            }
            else
            {
                DomainEvents.Raise(new CatalogEntryFound(entry));
            }
        }

        public HashAlgorithm GetHashAlgorithm(string hashFunctionName)
        {
            var algorithm = DataService.GetHashAlgorithm(hashFunctionName);

            if (algorithm == null)
            {
                throw new InvalidOperationException($"Unknown hash function {hashFunctionName}");
            }

            return algorithm;
        }

        private void OutputFiles(IEnumerable<File> files, int level = 0)
        {
            foreach (var file in files)
            {
                DomainEvents.Raise(new TreeItemEnumerated(file, level));

                OutputFiles(file.Contents, level + 1);
            }
        }

        public void Execute(ICommand command)
        {
            try
            {
                command.Execute(this);
            }
            catch (Exception e)
            {
                DomainEvents.Raise(new ErrorEvent(e));
            }
        }

        public string GetBackupFilePath(string hash)
        {
            var backupFileDir = FilesystemService.CombinePath(FileStorageDirectory, hash.Substring(0, 2));
            return FilesystemService.CombinePath(backupFileDir, hash);
        }

        public string GenerateSnapshotFileName()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
        }

        public string GetBackupSnapshotDirectory(string snapshotName)
        {
            return FilesystemService.CombinePath(SnapshotsDirectory, snapshotName);
        }

        public string GetLastSnapshotFile(string snapshotName)
        {
            var dir = GetBackupSnapshotDirectory(snapshotName);

            if (!FilesystemService.DirectoryExists(dir))
            {
                return null;
            }

            var dirInfo = FilesystemService.GetDirectory(dir);
            var files = dirInfo.EnumerateFiles("????-??-??_??-??-??");

            return files.Max(f => f.Name);
        }

        public void SortTree(File directory)
        {
            directory.Contents.Sort(FileNameComparer);

            foreach (var f in directory.Contents)
            {
                SortTree(f);
            }
        }

        public File GetInnerFile(File container, string name)
        {
            var index = container.Contents.BinarySearch(new File(name), FileNameComparer);
            if (index < 0)
            {
                return null;
            }

            return container.Contents[index];
        }

        private IDataService DataService { get; }
        private IFilesystemService FilesystemService { get; }
        private IIndex<string, IHashFunction> HashFunctions { get; }
        private IComparer<File> FileNameComparer { get; } = new FileNameComparer();
    }
}
