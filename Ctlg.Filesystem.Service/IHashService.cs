namespace Ctlg.Filesystem.Service
{
    public interface IHashService
    {
        byte[] CalculateSha1(string path);
    }
}
