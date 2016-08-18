using Ctlg.Data.Model;
using Ctlg.Filesystem.Service;

namespace Ctlg.Service.Commands
{
    public class AddCommand: ICommand
    {
        public string Path { get; set; }
        
        public void Execute(ICtlgService svc)
        {
            _output = svc.Output;
            var di = svc.FilesystemService.GetDirectory(Path);
            var root = ParseDirectory(di);
            root.Name = di.Directory.FullPath;

            svc.DataService.AddDirectory(root);
            
            svc.DataService.SaveChanges();
        }

        private File ParseDirectory(IFilesystemDirectory fsDirectory)
        {
            var directory = fsDirectory.Directory;
            _output.WriteLine(directory.FullPath);

            foreach (var file in fsDirectory.EnumerateFiles())
            {
                _output.WriteLine(file.FullPath);
                directory.Contents.Add(file);
            }

            foreach (var dir in fsDirectory.EnumerateDirectories())
            {
                directory.Contents.Add(ParseDirectory(dir));
            }

            return directory;
        }

        private IOutput _output;
    }
}
