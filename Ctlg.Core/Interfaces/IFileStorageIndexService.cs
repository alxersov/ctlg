using System;
namespace Ctlg.Core.Interfaces
{
    public interface IFileStorageIndexService
    {
        IFileStorageIndex GetIndex(string backupRootDirectory);
    }
}
