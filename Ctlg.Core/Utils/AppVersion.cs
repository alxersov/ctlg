using System;
using System.Reflection;

namespace Ctlg.Core.Utils
{
    public static class AppVersion
    {
        public static string Version
        {
            get
            {
                return Assembly.GetEntryAssembly().GetName().Version.ToString();
            }
        }
    }
}
