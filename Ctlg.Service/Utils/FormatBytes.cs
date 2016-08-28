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
    }
}
