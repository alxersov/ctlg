using System;
using System.Collections.Generic;
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
        }

        public void ApplyDbMigrations()
        {
            DataService.ApplyDbMigrations();
        }

        public void AddDirectory(string path, string searchPattern, string hashFunctionName)
        {
            hashFunctionName = hashFunctionName ?? "SHA-1";
            hashFunctionName = hashFunctionName.ToUpperInvariant();

            IHashFunction hashFunction;
            if (!HashFunctions.TryGetValue(hashFunctionName, out hashFunction))
            {
                throw new Exception($"Unsupported hash function {hashFunctionName}");
            }

            var hashAlgorithm = DataService.GetHashAlgorithm(hashFunctionName);

            if (string.IsNullOrEmpty(searchPattern))
            {
                searchPattern = "*";
            }

            var di = FilesystemService.GetDirectory(path);
            var root = ParseDirectory(di, searchPattern);
            root.Name = di.Directory.FullPath;

            ProcessTree(root, hashFunction, hashAlgorithm.HashAlgorithmId);

            DataService.AddDirectory(root);

            DataService.SaveChanges();

            DomainEvents.Raise(new AddCommandFinished());
        }

        public void ListFiles()
        {
            OutputFiles(DataService.GetFiles());
        }

        public void FindFiles(byte[] hash)
        {
            var files = DataService.GetFiles(hash);

            foreach (var f in files)
            {
                DomainEvents.Raise(new FileFoundInDb(f));
            }
        }

        private void OutputFiles(IEnumerable<File> files, int level = 0)
        {
            foreach (var file in files)
            {
                DomainEvents.Raise(new TreeItemEnumerated(file, level));

                OutputFiles(file.Contents, level + 1);
            }
        }

        private File ParseDirectory(IFilesystemDirectory fsDirectory, string searchPattern)
        {
            var directory = fsDirectory.Directory;

            DomainEvents.Raise(new DirectoryFound(directory.FullPath));

            foreach (var file in fsDirectory.EnumerateFiles(searchPattern))
            {
                DomainEvents.Raise(new FileFound(file.FullPath));

                directory.Contents.Add(file);
            }

            foreach (var dir in fsDirectory.EnumerateDirectories())
            {
                directory.Contents.Add(ParseDirectory(dir, searchPattern));
            }

            return directory;
        }

        private void ProcessTree(File directory, IHashFunction hashFunction, int hashAlgorithmId)
        {
            foreach (var file in directory.Contents)
            {
                if (file.IsDirectory)
                {
                    ProcessTree(file, hashFunction, hashAlgorithmId);
                }
                else
                {
                    CalculateHashes(file, hashFunction, hashAlgorithmId);

                    if (FilesystemService.IsArchiveExtension(file.FullPath))
                    {
                        ProcessArchive(file);
                    }
                }
            }
        }

        private void CalculateHashes(File file, IHashFunction hashFunction, int hashAlgorithmId)
        {
            try
            {
                using (var stream = FilesystemService.OpenFileForRead(file.FullPath))
                {
                    var hash = hashFunction.CalculateHash(stream);

                    DomainEvents.Raise(new HashCalculated(file.FullPath, hash));

                    file.Hashes.Add(new Hash(hashAlgorithmId, hash));
                }
            }
            catch (Exception e)
            {
                DomainEvents.Raise(new ExceptionEvent(e));
            }
        }

        private void ProcessArchive(File file)
        {
            try
            {
                using (var stream = FilesystemService.OpenFileForRead(file.FullPath))
                {
                    var archive = FilesystemService.OpenArchive(stream);

                    DomainEvents.Raise(new ArchiveFound(file.FullPath));

                    foreach (var entry in archive.EnumerateEntries())
                    {
                        file.Contents.Add(entry);

                        DomainEvents.Raise(new ArchiveEntryFound(entry.Name));
                    }
                }
            }
            catch (Exception e)
            {
                DomainEvents.Raise(new ExceptionEvent(e));
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
                DomainEvents.Raise(new ExceptionEvent(e));
            }
        }

        private IDataService DataService { get; }
        private IFilesystemService FilesystemService { get; }
        private IIndex<string, IHashFunction> HashFunctions { get; }
    }
}
