using System;
namespace Ctlg.Core.Interfaces
{
    public interface IHashingService
    {
        HashCalculator CreateHashCalculator(string algorithmName);
    }
}
