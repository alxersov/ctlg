using System;
namespace Ctlg.Service.Utils
{
    public static class FileSize
    {
        private static string[] Prefixes = new string[]{"", "k", "M", "G", "T", "P"};

        public static string Format(long size)
        {
            double d = size;
            var prefixIndex = 0;
            while (1024 <= d && prefixIndex < Prefixes.Length - 1)
            {
                d /= 1024;
                ++prefixIndex;
            }

            return $"{d:G4}{Prefixes[prefixIndex]}";
        }
    }
}
