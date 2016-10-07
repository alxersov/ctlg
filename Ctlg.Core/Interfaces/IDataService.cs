using System.Collections.Generic;

namespace Ctlg.Core.Interfaces
{
    public interface IDataService
    {
        void ApplyDbMigrations();
        void AddDirectory(File directory);
        File GetCatalogEntry(int catalogEntryId);
        IEnumerable<File> GetFiles();
        IEnumerable<File> GetFiles(byte[] hash);
        HashAlgorithm GetHashAlgorithm(string name);

        void SaveChanges();
    }
}
