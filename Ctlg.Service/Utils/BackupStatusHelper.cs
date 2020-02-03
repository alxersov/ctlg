using System;
using Ctlg.Core;

namespace Ctlg.Service.Utils
{
    public static class BackupFileStatusHelper
    {
        public static bool IsNotFound(this BackupFileStatus status)
        {
            return !status.HasFlag(BackupFileStatus.FoundInIndex) &&
                !status.HasFlag(BackupFileStatus.FoundInStorage);
        }
    }
}
