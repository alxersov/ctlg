using System.Collections.Generic;
using Ctlg.Core;

namespace Ctlg.Data
{
    public interface IDataService
    {
        void ApplyDbMigrations();
        void AddDirectory(File directory);
        IEnumerable<File> GetFiles();
        IEnumerable<File> GetFiles(byte[] hash);
        HashAlgorithm GetHashAlgorithm(string name);

        void SaveChanges();
    }
}
