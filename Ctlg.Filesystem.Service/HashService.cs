using System.IO;
using System.Security.Cryptography;

namespace Ctlg.Filesystem.Service
{
    public class HashService: IHashService
    {
        public byte[] CalculateSha1(string path)
        {
            var sha1 = new SHA1Cng();

            using (var stream = File.OpenRead(path))
            {
                return sha1.ComputeHash(stream);
            }
        }

    }
}
