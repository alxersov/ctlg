using Ctlg.Core;
using Ctlg.Service.Commands;

namespace Ctlg.Service
{
    public interface ICtlgService
    {
        void Execute(ICommand command);
        void ApplyDbMigrations();

        void AddDirectory(string path, string searchPattern, string hashFunctionName);
        void ListFiles();
        void FindFiles(Hash hash, long? size, string namePattern);
        void Show(int catalgoEntryId);

        HashAlgorithm GetHashAlgorithm(string hashAlgorithmName);
    }
}
