namespace Ctlg.Filesystem.Service
{
    public interface IFilesystemService
    {
        IFilesystemDirectory GetDirectory(string path);
        byte[] CalculateSha1(string path);
    }
}
