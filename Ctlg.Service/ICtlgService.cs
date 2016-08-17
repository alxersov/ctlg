using Ctlg.Data.Service;
using Ctlg.Filesystem.Service;
using Ctlg.Service.Commands;

namespace Ctlg.Service
{
    public interface ICtlgService
    {
        IDataService DataService { get; }
        IFilesystemService FilesystemService { get; }

        void Execute(ICommand command);
        void ApplyDbMigrations();
    }
}
