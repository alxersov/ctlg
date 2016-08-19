using Ctlg.Service.Commands;

namespace Ctlg.Service
{
    public interface ICtlgService
    {
        void Execute(ICommand command);
        void ApplyDbMigrations();

        void AddDirectory(string path);
        void ListFiles();
    }
}
