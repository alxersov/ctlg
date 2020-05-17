using System;
using Ctlg.Service.Utils;

namespace Ctlg.Core.Utils
{
    public static class RandomUtils
    {
        public static string RandomHexString(int sizeBytes)
        {
            var rnd = new Random();
            var bytes = new byte[sizeBytes];
            rnd.NextBytes(bytes);

            return FormatBytes.ToHexString(bytes);
        }
    }
}
