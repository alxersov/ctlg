using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Commands;

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
        void SortTree(File directory);
        File GetInnerFile(File container, string name);

        string CurrentDirectory { get; }
        string FileStorageDirectory { get; }
        string IndexPath { get; }
    }
}
