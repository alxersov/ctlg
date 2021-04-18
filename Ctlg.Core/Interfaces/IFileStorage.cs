using System;
using System.Collections.Generic;

namespace Ctlg.Core.Interfaces
{
    public interface IFileStorage
    {
        void AddFile(File file, byte[] hash);
        byte[] AddFileFromStorage(SnapshotRecord snapshotRecord, IFileStorage sourceStorage);
        void CopyFileTo(string hash, string destinationPath);
        void CopyFileTo(SnapshotRecord snapshotRecord, string destinationPath);
        IEnumerable<byte[]> GetAllHashes();
        bool IsFileInStorage(SnapshotRecord snapshotRecord);
        bool IsFileInStorage(File file, byte[] hash);
        bool VerifyFileByHash(byte[] hash);
        string HashAlgorithmName { get; }
    }
}
