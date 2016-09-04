using System;
using System.Collections.Generic;
using System.Linq;
using Ctlg.Data.Model;
using Ctlg.Data.Service;
using Ctlg.Filesystem.Service;
using Ctlg.Service.Commands;
using Ctlg.Service.Utils;

namespace Ctlg.Service
{
    public class CtlgService : ICtlgService
    {
        public CtlgService(IDataService dataService, IFilesystemService filesystemService, IHashService hashService, IOutput output)
        {
            DataService = dataService;
            FilesystemService = filesystemService;
            HashService = hashService;
            Output = output;
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
                Output.WriteLine(string.Format("{0} {1}", f.BuildFullPath(), f.RecordUpdatedDateTime));
            }
        }

        private void OutputFiles(IList<File> files, int level = 0)
        {
            var padding = "".PadLeft(level*4);
            foreach (var file in files)
            {
                var hashes = string.Join(" ", file.Hashes.Select(h => FormatBytes.ToHexString(h.Value)));

                if (string.IsNullOrEmpty(hashes))
                {
                    hashes = "".PadLeft(40);
                }

                Output.WriteLine(string.Format("{0} {1} {2}", hashes, padding, file.Name));
                OutputFiles(file.Contents, level + 1);
            }
        }

        private File ParseDirectory(IFilesystemDirectory fsDirectory)
        {
            var directory = fsDirectory.Directory;
            Output.WriteLine(directory.FullPath);

            foreach (var file in fsDirectory.EnumerateFiles())
            {
                Output.WriteLine(file.FullPath);
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

                            Output.WriteLine(string.Format("{0} {1}",
                                FormatBytes.ToHexString(hash),
                                file.FullPath));

                            file.Hashes.Add(new Hash(1, hash));
                        }
                    }
                    catch (Exception e)
                    {
                        Output.WriteLine(e.ToString());
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
        private IOutput Output { get; }
    }
}
