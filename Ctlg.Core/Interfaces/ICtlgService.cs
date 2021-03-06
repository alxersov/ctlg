﻿namespace Ctlg.Core.Interfaces
{
    public interface ICtlgService
    {
        void ApplyDbMigrations();

        void ListFiles();
        void FindFiles(Hash hash, long? size, string namePattern);
        void Show(int catalgoEntryId);

        HashAlgorithm GetHashAlgorithm(string hashAlgorithmName);
    }
}
