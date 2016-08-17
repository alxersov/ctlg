using System.Data;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SQLite;
using Ctlg.Data.Model;

namespace Ctlg.Data.Service
{
    public class CtlgContext : DbContext, ICtlgContext
    {
        public DbSet<File> Files { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<File>()
                .HasOptional(f => f.ParentFile)
                .WithMany(f => f.Contents)
                .HasForeignKey(f => f.ParentFileId);

            base.OnModelCreating(modelBuilder);
        }

        public int DbVersion
        {
            get
            {
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA user_version;";
                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        return reader.GetInt32(0);
                    }
                }
            }
        }

        public SQLiteConnection Connection
        {
            get
            {
                var connection = (SQLiteConnection)Database.Connection;

                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                return connection;
            }
        }

        public void ApplyMigration(string migration, int dbVersion)
        {
            using (var transaction = Connection.BeginTransaction())
            {
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText = migration;
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText = string.Format("PRAGMA user_version = {0}", dbVersion);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }
    }
}
