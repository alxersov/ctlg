using System;
using System.Collections.Generic;
using Autofac.Features.Indexed;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;
using File = Ctlg.Core.File;

namespace Ctlg.Service.Services
{
    public sealed class CtlgService : ICtlgService
    {
        public CtlgService(IDataService dataService, IFilesystemService filesystemService,
            IIndex<string, IHashFunction> hashFunction)
        {
            DataService = dataService;
            FilesystemService = filesystemService;
            HashFunctions = hashFunction;
        }


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

        private IDataService DataService { get; }
        private IFilesystemService FilesystemService { get; }
        private IIndex<string, IHashFunction> HashFunctions { get; set; }
        private IComparer<File> FileNameComparer { get; } = new FileNameComparer();
    }
}
