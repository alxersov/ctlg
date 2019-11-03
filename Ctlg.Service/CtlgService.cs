using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Features.Indexed;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;
using File = Ctlg.Core.File;

namespace Ctlg.Service
{
    public class CtlgService : ICtlgService
    {
        public CtlgService(IDataService dataService, IFilesystemService filesystemService,
            ISnapshotService snapshotService, IIndexService indexService,
            IIndex<string, IHashFunction> hashFunction, IComponentContext componentContext)
        {
            DataService = dataService;
            FilesystemService = filesystemService;
            SnapshotService = snapshotService;
            IndexService = indexService;
            HashFunctions = hashFunction;
            ComponentContext = componentContext;

            CurrentDirectory = FilesystemService.GetCurrentDirectory();
            FileStorageDirectory = FilesystemService.CombinePath(CurrentDirectory, "file_storage");
            IndexPath = FilesystemService.CombinePath(CurrentDirectory, "index.bin");
        }

        public string CurrentDirectory { get; private set; }
        public string FileStorageDirectory {get; private set; }
        public string IndexPath { get; private set; }

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

        public IHashFunction GetHashFunction(string name)
        {
            var canonicalName = name.ToUpperInvariant();
            if (!HashFunctions.TryGetValue(canonicalName, out IHashFunction hashFunction))
            {
                throw new Exception($"Unsupported hash function {name}");
            }

            return hashFunction;
        }

        private void OutputFiles(IEnumerable<File> files, int level = 0)
        {
            foreach (var file in files)
            {
                DomainEvents.Raise(new TreeItemEnumerated(file, level));

                OutputFiles(file.Contents, level + 1);
            }
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

            var hash = file.Hashes.First(h => h.HashAlgorithmId == (int)HashAlgorithmId.SHA256);
            IndexService.Add(hash.Value);
        }

        public Hash CalculateHashForFile(File file, IHashFunction hashFunction)
        {
            using (var stream = FilesystemService.OpenFileForRead(file.FullPath))
            {
                var hash = hashFunction.CalculateHash(stream);

                file.Hashes.Add(hash);

                return hash;
            }
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

        public IBackupWriter CreateBackupWriter(string name, bool shouldUseIndex)
        {
            NamedParameter[] parameters =
            {
                new NamedParameter("shouldUseIndex", shouldUseIndex),
                new NamedParameter("hashFunction", GetHashFunction("SHA-256")),
                new NamedParameter("writer", SnapshotService.CreateSnapshotWriter(name))
            };

            return ComponentContext.Resolve<BackupWriter>(parameters);
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

        private IDataService DataService { get; }
        private IFilesystemService FilesystemService { get; }
        private ISnapshotService SnapshotService { get; set; }
        private IIndexService IndexService { get; set; }
        private IIndex<string, IHashFunction> HashFunctions { get; set; }
        private IComponentContext ComponentContext { get; set; }
        private IComparer<File> FileNameComparer { get; } = new FileNameComparer();
    }
}
