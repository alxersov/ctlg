using Ctlg.Data.Model;

namespace Ctlg.Data.Service
{
    public interface IDataService
    {
        void ApplyDbMigrations();
        void AddDirectory(File directory);
        void SaveChanges();
    }
}
