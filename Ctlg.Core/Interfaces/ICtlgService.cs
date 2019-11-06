using Ctlg.Core;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service
{
    public interface ICtlgService
    {
        void ApplyDbMigrations();

        void ListFiles();
        void FindFiles(Hash hash, long? size, string namePattern);
        void Show(int catalgoEntryId);

        HashAlgorithm GetHashAlgorithm(string hashAlgorithmName);

        IHashFunction GetHashFunction(string name);

        void SortTree(File directory);
        Hash CalculateHashForFile(File file, IHashFunction hashFunction);
        File GetInnerFile(File container, string name);
        IBackupWriter CreateBackupWriter(string name, bool shouldUseIndex);
    }
}
