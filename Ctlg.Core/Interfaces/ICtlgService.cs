namespace Ctlg.Core.Interfaces
{
    public interface ICtlgService
    {
        void ApplyDbMigrations();

        HashAlgorithm GetHashAlgorithm(string hashAlgorithmName);
    }
}
