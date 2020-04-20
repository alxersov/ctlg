﻿using System;
namespace Ctlg.Core.Interfaces
{
    public interface IFileStorageService
    {
        IFileStorage GetFileStorage(string backupRootDirectory, string hashAlgorithmName);
    }
}
