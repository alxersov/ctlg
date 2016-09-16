using System;
using System.Collections.Generic;
using Autofac.Features.Indexed;
using Ctlg.Data.Model;
using Ctlg.Data.Service;
using Ctlg.Filesystem.Service;
using Ctlg.Service.Commands;
using Ctlg.Service.Events;

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

        public void AddDirectory(string path, string searchPattern)
        {
            var hashFunction = HashFunctions["SHA-1"];

            if (string.IsNullOrEmpty(searchPattern))
            {
                searchPattern = "*";
            }

            var di = FilesystemService.GetDirectory(path);
            var root = ParseDirectory(di, searchPattern);
            root.Name = di.Directory.FullPath;

            CalculateHashes(root, hashFunction);

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

        private void CalculateHashes(File directory, IHashFunction hashFunction)
        {
            foreach (var file in directory.Contents)
            {
                if (file.IsDirectory)
                {
                    CalculateHashes(file, hashFunction);
                }
                else
                {
                    try
                    {
                        using (var stream = FilesystemService.OpenFileForRead(file.FullPath))
                        {
                            var hash = hashFunction.CalculateHash(stream);

                            DomainEvents.Raise(new HashCalculated(file.FullPath, hash));

                            file.Hashes.Add(new Hash(1, hash));
                        }
                    }
                    catch (Exception e)
                    {
                        DomainEvents.Raise(new ExceptionEvent(e));
                    }
                }
            }
        }


        public void Execute(ICommand command)
        {
            command.Execute(this);
        }

        private IDataService DataService { get; }
        private IFilesystemService FilesystemService { get; }
        private IIndex<string, IHashFunction> HashFunctions { get; }
    }
}
