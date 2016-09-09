using System;
using System.Collections.Generic;
using Ctlg.Data.Model;
using Ctlg.Data.Service;
using Ctlg.Filesystem.Service;
using Ctlg.Service.Commands;
using Ctlg.Service.Events;

namespace Ctlg.Service
{
    public class CtlgService : ICtlgService
    {
        public CtlgService(IDataService dataService, IFilesystemService filesystemService, IHashService hashService)
        {
            DataService = dataService;
            FilesystemService = filesystemService;
            HashService = hashService;
        }

        public void ApplyDbMigrations()
        {
            DataService.ApplyDbMigrations();
        }

        public void AddDirectory(string path)
        {
            var di = FilesystemService.GetDirectory(path);
            var root = ParseDirectory(di);
            root.Name = di.Directory.FullPath;

            CalculateHashes(root);

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

        private File ParseDirectory(IFilesystemDirectory fsDirectory)
        {
            var directory = fsDirectory.Directory;

            DomainEvents.Raise(new DirectoryFound(directory.FullPath));

            foreach (var file in fsDirectory.EnumerateFiles())
            {
                DomainEvents.Raise(new FileFound(file.FullPath));

                directory.Contents.Add(file);
            }

            foreach (var dir in fsDirectory.EnumerateDirectories())
            {
                directory.Contents.Add(ParseDirectory(dir));
            }

            return directory;
        }

        private void CalculateHashes(File directory)
        {
            foreach (var file in directory.Contents)
            {
                if (file.IsDirectory)
                {
                    CalculateHashes(file);
                }
                else
                {
                    try
                    {
                        using (var stream = FilesystemService.OpenFileForRead(file.FullPath))
                        {
                            var hash = HashService.CalculateSha1(stream);

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
        private IHashService HashService { get; }
    }
}
