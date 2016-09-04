using System.Collections.Generic;
using Ctlg.Data.Model;

namespace Ctlg.Data.Service
{
    public interface IDataService
    {
        void ApplyDbMigrations();
        void AddDirectory(File directory);
        IList<File> GetFiles();
        IEnumerable<File> GetFiles(byte[] hash);

        void SaveChanges();
    }
}
