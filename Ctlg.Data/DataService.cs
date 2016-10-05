using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Db.Migrations;

namespace Ctlg.Data
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

        public IEnumerable<File> GetFiles()
        {
            var rootDirs = _ctlgContext.Files.Where(f => f.ParentFile == null).AsNoTracking();
            foreach (var dir in rootDirs)
            {
                LoadContents(dir);
                yield return dir;
            }
        }

        public IEnumerable<File> GetFiles(byte[] hash)
        {
            var foundFiles = _ctlgContext.Files.Where(f => f.Hashes.Any(h => h.Value == hash));

            foreach (var file in foundFiles)
            {
                LoadParentFile(file);
                yield return file;
            }
        }

        public HashAlgorithm GetHashAlgorithm(string name)
        {
            return _ctlgContext.HashAlgorithm.First(a => a.Name == name);
        }

        private void LoadContents(File file)
        {
            file.Contents = _ctlgContext.Files.Where(f => f.ParentFileId == file.FileId).AsNoTracking().ToList();

            foreach (var child in file.Contents)
            {
                LoadContents(child);
            }
        }

        private void LoadParentFile(File file)
        {
            if (file.ParentFileId != null &&  file.ParentFile == null)
            {
                _ctlgContext.Entry(file).Reference(f => f.ParentFile).Load();

                LoadParentFile(file.ParentFile);
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
