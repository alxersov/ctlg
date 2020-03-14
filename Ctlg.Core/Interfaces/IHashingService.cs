using System;
namespace Ctlg.Core.Interfaces
{
    public interface IHashingService
    {
        IHashFunction GetHashFunction(string name);
        Hash CalculateHashForFile(File file, IHashFunction hashFunction);
    }
}
