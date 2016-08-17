using System.Data.Entity;
using System.Data.SQLite;
using Ctlg.Data.Model;

namespace Ctlg.Data.Service
{
    public interface ICtlgContext
    {
        SQLiteConnection Connection { get; }
        int DbVersion { get; }
        DbSet<File> Files { get; set; }

        void ApplyMigration(string migration, int dbVersion);
        int SaveChanges();
    }
}
