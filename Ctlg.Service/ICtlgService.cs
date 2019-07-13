using Ctlg.Core;
using Ctlg.Service.Commands;

namespace Ctlg.Service
{
    public interface ICtlgService
    {
        void Execute(ICommand command);
        void ApplyDbMigrations();

        void ListFiles();
        void FindFiles(Hash hash, long? size, string namePattern);
        void Show(int catalgoEntryId);

        HashAlgorithm GetHashAlgorithm(string hashAlgorithmName);

        string GetBackupFilePath(string hash);
        void SortTree(File directory);
        File GetInnerFile(File container, string name);

        string CurrentDirectory { get; }
        string FileStorageDirectory { get; }
    }
}
