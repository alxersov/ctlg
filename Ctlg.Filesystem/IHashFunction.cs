using System.IO;

namespace Ctlg.Filesystem
{
    public interface IHashFunction
    {
        string Name { get; }
        byte[] CalculateHash(Stream stream);
    }
}
