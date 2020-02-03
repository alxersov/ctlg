using System;
namespace Ctlg.Core
{
    [Flags]
    public enum BackupFileStatus
    {
        FoundInIndex = 1,
        FoundInStorage = 2,
        HashRecalculated = 8
    }
}
