using System;
using System.Collections.Generic;

namespace Ctlg.Service
{
    public class ByteArrayComparer : IComparer<byte[]>
    {
        public int Compare(byte[] x, byte[] y)
        {
            return ByteArrayComparer.DoCompare(x, y);
        }

        public static bool AreEqual(byte[] x, byte[] y)
        {
            return DoCompare(x, y) == 0;
        }

        private static int DoCompare(byte[] x, byte[] y)
        {
            if (x.Length < y.Length)
            {
                return -1;
            }
            if (x.Length > y.Length)
            {
                return 1;
            }

            var length = x.Length;
            for (var i = 0; i < length; ++i)
            {
                if (x[i] < y[i])
                {
                    return -1;
                }
                if (x[i] > y[i])
                {
                    return 1;
                }
            }

            return 0;
        }
    }
}
