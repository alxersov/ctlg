using System.Collections.Generic;
using System.Data.Entity;
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
            var dict = new Dictionary<Hash, Hash>();

            LoadExistingHashes(directory, dict);
            _ctlgContext.Files.Add(directory);
        }

        private void LoadExistingHashes(File file, Dictionary<Hash, Hash> dict)
        {
            for (int i = 0; i < file.Hashes.Count; ++i)
            {
                var hash = file.Hashes[i];
                var hashInDb =
                    _ctlgContext.Hashes.FirstOrDefault(
                        h => h.HashAlgorithmId == hash.HashAlgorithmId && h.Value == hash.Value);
                if (hashInDb != null)
                {
                    file.Hashes[i] = hashInDb;
                }
                else
                {
                    Hash hashProcessed;
                    if (dict.TryGetValue(hash, out hashProcessed))
                    {
                        file.Hashes[i] = hashProcessed;
                    }
                    else
                    {
                        dict.Add(hash, hash);
                    }
                }
            }

            foreach (var f in file.Contents)
            {
                LoadExistingHashes(f, dict);
            }
        }

        public IList<File> GetFiles()
        {
            var rootDirs = _ctlgContext.Files.Where(f => f.ParentFile == null).Include(f => f.Hashes).ToList();
            LoadContents(rootDirs);
            return rootDirs;
        }

        private void LoadContents(IList<File> rootDirs)
        {
            foreach (var dir in rootDirs)
            {
                _ctlgContext.Entry(dir).Collection(d => d.Contents).Load();
                _ctlgContext.Entry(dir).Collection(d => d.Hashes).Load();
                LoadContents(dir.Contents);
            }
        }

        public void SaveChanges()
        {
            _ctlgContext.SaveChanges();
        }

        protected ICtlgContext _ctlgContext;
        protected IMigrationService _migrationService;

        public const int RequiredDbVersion = 2;
    }
}
