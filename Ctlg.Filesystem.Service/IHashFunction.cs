using System.IO;

namespace Ctlg.Filesystem.Service
{
    public interface IHashFunction
    {
        string Name { get; }
        byte[] CalculateHash(Stream stream);
    }
}
