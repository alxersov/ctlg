using Ctlg.Data.Model;
using Ctlg.Data.Service;
using Ctlg.Filesystem.Service;
using Ctlg.Service.Commands;

namespace Ctlg.Service
{
    public class CtlgService : ICtlgService
    {
        public CtlgService(IDataService dataService, IFilesystemService filesystemService, IOutput output)
        {
            DataService = dataService;
            FilesystemService = filesystemService;
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

            DataService.AddDirectory(root);

            DataService.SaveChanges();
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


        public void Execute(ICommand command)
        {
            command.Execute(this);
        }

        private IDataService DataService { get; }
        private IFilesystemService FilesystemService { get; }
        private IOutput Output { get; }
    }
}
