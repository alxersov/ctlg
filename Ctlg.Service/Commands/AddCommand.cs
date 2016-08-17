﻿using Ctlg.Data.Model;
using Ctlg.Filesystem.Service;

namespace Ctlg.Service.Commands
{
    public class AddCommand: ICommand
    {
        public string Path { get; set; }
        
        public void Execute(ICtlgService svc)
        {
            var di = svc.FilesystemService.GetDirectory(Path);
            var root = ParseDirectory(di);
            root.Name = di.FullPath;

            svc.DataService.AddDirectory(root);
            
            svc.DataService.SaveChanges();
        }

        private static File ParseDirectory(IFilesystemDirectory fsDirectory)
        {
            var directory = fsDirectory.File;

            foreach (var file in fsDirectory.EnumerateFiles())
            {
                directory.Contents.Add(file.File);
            }

            foreach (var dir in fsDirectory.EnumerateDirectories())
            {
                directory.Contents.Add(ParseDirectory(dir));
            }

            return directory;
        }
    }
}
