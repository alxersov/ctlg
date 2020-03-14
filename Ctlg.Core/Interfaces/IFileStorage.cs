using System;
using System.Collections.Generic;

namespace Ctlg.Core.Interfaces
{
    public interface IFileStorage
    {
        void AddFile(File file);
        void AddFileFromStorage(File file, IFileStorage sourceStorage);
        void CopyFileTo(string hash, string destinationPath);
        IEnumerable<byte[]> GetAllHashes();
        bool IsFileInStorage(File file);
    }
}
