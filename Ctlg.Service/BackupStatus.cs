using System;
namespace Ctlg.Service
{
    [Flags]
    public enum BackupFileStatus
    {
        FoundInIndex = 1,
        FoundInStorage = 2,
        HashRecalculated = 8
    }
}
