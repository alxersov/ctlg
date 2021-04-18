using System.IO;

namespace Ctlg.Core.Interfaces
{
    public interface IHashFunction
    {
        byte[] Calculate(Stream stream);
        int HashSize { get; }
    }
}
