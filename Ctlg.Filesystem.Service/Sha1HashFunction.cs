using System.IO;
using System.Security.Cryptography;

namespace Ctlg.Filesystem.Service
{
    public class Sha1HashFunction: IHashFunction
    {
        public Sha1HashFunction(SHA1 sha1)
        {
            Sha1 = sha1;
        }

        public string Name => "SHA-1";

        public byte[] CalculateHash(Stream stream)
        {
            return Sha1.ComputeHash(stream);
        }

        private SHA1 Sha1 { get; }
    }
}
