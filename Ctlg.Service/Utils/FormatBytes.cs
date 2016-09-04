using System;
using System.Text;

namespace Ctlg.Service.Utils
{
    public static class FormatBytes
    {
        public static string ToHexString(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }

        public static byte[] ToByteArray(string hexString)
        {
            int n = hexString.Length;
            byte[] bytes = new byte[n/2];
            for (int i = 0; i < n; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }
                
            return bytes;
        }
    }
}
