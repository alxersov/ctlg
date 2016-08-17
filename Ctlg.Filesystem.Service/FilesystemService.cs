namespace Ctlg.Filesystem.Service
{
    public class FilesystemService : IFilesystemService
    {
        public IFilesystemDirectory GetDirectory(string path)
        {
            return new FilesystemDirectory(path);
        }
    }
}
