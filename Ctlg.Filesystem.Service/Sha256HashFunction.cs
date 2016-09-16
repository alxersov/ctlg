using System.IO;
using System.Security.Cryptography;

namespace Ctlg.Filesystem.Service
{
    public class Sha256HashFunction : IHashFunction
    {
        public Sha256HashFunction(SHA256 sha256)
        {
            Sha256 = sha256;
        }

        public string Name => "SHA-256";

        public byte[] CalculateHash(Stream stream)
        {
            return Sha256.ComputeHash(stream);
        }

        private SHA256 Sha256 { get; set; }
    }
}
