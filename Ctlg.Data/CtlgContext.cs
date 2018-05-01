using System.Data;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SQLite;
using System.Diagnostics;
using Ctlg.Core;

namespace Ctlg.Data
{
    public class CtlgContext : DbContext, ICtlgContext
    {
        public CtlgContext()
        {
            Database.Log = s => Debug.WriteLine(s);
        }

        public DbSet<File> Files { get; set; }
        public DbSet<Hash> Hashes { get; set; }
        public DbSet<HashAlgorithm> HashAlgorithm { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<File>()
                .Property(f => f.Name)
                .IsRequired();

            modelBuilder.Entity<File>()
                .HasOptional(f => f.ParentFile)
                .WithMany(f => f.Contents)
                .HasForeignKey(f => f.ParentFileId);

            modelBuilder.Entity<File>()
                .HasMany(f => f.Hashes)
                .WithMany()
                .Map(m =>
                {
                    m.ToTable("FileHash");
                    m.MapLeftKey("FileId");
                    m.MapRightKey("HashId");
                });

            modelBuilder.Entity<File>().Ignore(f => f.RelativePath);
            modelBuilder.Entity<File>().Ignore(f => f.FullPath);

            modelBuilder.Entity<Hash>()
                .Property(h => h.Value)
                .IsRequired();

            modelBuilder.Entity<Hash>()
                .HasRequired(h => h.HashAlgorithm).WithMany().HasForeignKey(h => h.HashAlgorithmId);

            modelBuilder.Entity<HashAlgorithm>()
                .Property(h => h.Name)
                .IsRequired();

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
