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

        string GetBackupFilePath(string hash);
        void AddFileToStorage(File file);
        void SortTree(File directory);
        Hash CalculateHashForFile(File file, IHashFunction hashFunction);
        bool IsFileInStorage(File file);
        File GetInnerFile(File container, string name);
        IBackupWriter CreateBackupWriter(string name, bool shouldUseIndex);

        string CurrentDirectory { get; }
        string FileStorageDirectory { get; }
        string IndexPath { get; }
    }
}
