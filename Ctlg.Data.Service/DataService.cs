using System.Collections.Generic;
using System.Linq;
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

        public IList<File> GetFiles()
        {
            var rootDirs = _ctlgContext.Files.Where(f => f.ParentFile == null).ToList();
            LoadContents(rootDirs);
            return rootDirs;
        }

        private void LoadContents(IList<File> rootDirs)
        {
            foreach (var dir in rootDirs)
            {
                _ctlgContext.Entry(dir).Collection(d => d.Contents).Load();
                LoadContents(dir.Contents);
            }
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
