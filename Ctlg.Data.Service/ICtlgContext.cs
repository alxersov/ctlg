using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SQLite;
using Ctlg.Data.Model;

namespace Ctlg.Data.Service
{
    public interface ICtlgContext
    {
        SQLiteConnection Connection { get; }
        int DbVersion { get; }

        DbSet<File> Files { get; set; }
        DbSet<Hash> Hashes { get; set; }

        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        DbEntityEntry Entry(object entity);


        void ApplyMigration(string migration, int dbVersion);
        int SaveChanges();
    }
}
