using System.IO;

namespace Ctlg.Core.Interfaces
{
    public interface IHashFunction
    {
        string Name { get; }
        Hash CalculateHash(Stream stream);
    }
}
