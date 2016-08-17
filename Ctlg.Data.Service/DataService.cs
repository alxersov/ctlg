using Ctlg.Data.Model;
using Ctlg.Db.Migrations;

namespace Ctlg.Data.Service
{
    public class DataService : IDataService
    {
        public DataService(IMigrationService migrationService, ICtlgContext ctlgContext)
        {
            _migrationService = migrationService;
            _ctlgContext = ctlgContext;
        }

        public void ApplyDbMigrations()
        {
            var version = _ctlgContext.DbVersion;
            while (version < RequiredDbVersion)
            {
                ++version;
                var migration = _migrationService.LoadMigration(version);
                _ctlgContext.ApplyMigration(migration, version);
            }
        }

        public void AddDirectory(File directory)
        {
            _ctlgContext.Files.Add(directory);
        }

        public void SaveChanges()
        {
            _ctlgContext.SaveChanges();
        }

        protected ICtlgContext _ctlgContext;
        protected IMigrationService _migrationService;

        public const int RequiredDbVersion = 1;
    }
}
