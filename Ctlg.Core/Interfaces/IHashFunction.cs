using System.IO;

namespace Ctlg.Core.Interfaces
{
    public interface IHashFunction
    {
        string Name { get; }
        byte[] CalculateHash(Stream stream);
    }
}
