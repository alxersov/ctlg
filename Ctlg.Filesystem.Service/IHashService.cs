using System.IO;

namespace Ctlg.Filesystem.Service
{
    public interface IHashService
    {
        byte[] CalculateSha1(Stream stream);
    }
}
