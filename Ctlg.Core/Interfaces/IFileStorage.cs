using System;
using System.Collections.Generic;

namespace Ctlg.Core.Interfaces
{
    public interface IFileStorage
    {
        void AddFile(File file, byte[] hash);
        void AddFileFromStorage(File file, IFileStorage sourceStorage);
        void CopyFileTo(string hash, string destinationPath);
        void CopyFileTo(File file, string destinationPath);
        IEnumerable<byte[]> GetAllHashes();
        bool IsFileInStorage(File file);
        bool IsFileInStorage(File file, byte[] hash);
        bool VerifyFileByHash(byte[] hash);
    }
}
