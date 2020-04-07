using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Ctlg.Core;

namespace Ctlg.Data
{
    public interface ICtlgContext
    {
        int DbVersion { get; }

        DbSet<File> Files { get; set; }
        DbSet<Hash> Hashes { get; set; }
        DbSet<HashAlgorithm> HashAlgorithm { get; set; }

        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        DbEntityEntry Entry(object entity);

        void ApplyMigration(string migration, int dbVersion);
        int SaveChanges();
    }
}
