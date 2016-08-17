using Ctlg.Data.Service;
using Ctlg.Filesystem.Service;
using Ctlg.Service.Commands;

namespace Ctlg.Service
{
    public class CtlgService : ICtlgService
    {
        public CtlgService(IDataService dataService, IFilesystemService filesystemService)
        {
            DataService = dataService;
            FilesystemService = filesystemService;
        }

        public void ApplyDbMigrations()
        {
            DataService.ApplyDbMigrations();
        }

        public void Execute(ICommand command)
        {
            command.Execute(this);
        }


        public IDataService DataService { get; }
        public IFilesystemService FilesystemService { get; }
    }
}
