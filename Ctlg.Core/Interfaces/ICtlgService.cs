namespace Ctlg.Core.Interfaces
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
    }
}
