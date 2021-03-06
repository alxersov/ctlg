﻿using System;
using System.Collections.Generic;

namespace Ctlg.Core.Interfaces
{
    public interface IFileStorage
    {
        void AddFile(File file);
        void AddFileFromStorage(File file, IFileStorage sourceStorage);
        void CopyFileTo(string hash, string destinationPath);
        void CopyFileTo(File file, string destinationPath);
        IEnumerable<byte[]> GetAllHashes();
        bool IsFileInStorage(File file);
        bool VerifyFileByHash(byte[] hash);
    }
}
