using System;
namespace Ctlg.Core.Interfaces
{
    public interface IHashingService
    {
        HashCalculator CreateHashCalculator(HashAlgorithm algorithm);
        HashCalculator CreateHashCalculator(string algorithmName);
    }
}
