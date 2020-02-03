using System;

namespace Ctlg.Core.Interfaces
{
    public interface IFileStorageIndex
    {
        void Add(byte[] hash);
        bool IsInIndex(byte[] hash);
        void Load();
        void Save();
    }
}
