using System;

namespace Ctlg.Core.Utils
{
    public static class AppVersion
    {
        public static string Version { get; } = typeof(AppVersion).Assembly.GetName().Version.ToString();
    }
}
