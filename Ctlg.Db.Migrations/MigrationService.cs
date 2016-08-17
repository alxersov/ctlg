using System;
using System.IO;
using System.Reflection;

namespace Ctlg.Db.Migrations
{
    public class MigrationService : IMigrationService
    {
        public MigrationService()
        {
            _assembly = Assembly.GetAssembly(typeof(MigrationService));
        }

        public string LoadMigration(int dbVersion)
        {
            var resourceName = BuildResourceName(dbVersion);
            return LoadResource(resourceName);
        }

        private string BuildResourceName(int dbVersion)
        {
            return string.Format("Ctlg.Db.Migrations.Resources.Migration_{0:D3}.sql", dbVersion);
        }

        protected virtual string LoadResource(string name)
        {
            var stream = _assembly.GetManifestResourceStream(name);

            if (stream == null)
            {
                throw new Exception($"DB migration \"{name}\" not found.");
            }

            var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
        
        protected Assembly _assembly;
    }
}
