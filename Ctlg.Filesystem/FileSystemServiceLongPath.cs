using Pri.LongPath;
using Stream = System.IO.Stream;

namespace Ctlg.Filesystem
{
    public class FilesystemServiceLongPath : FilesystemServiceBase, IFilesystemService
    {
        public IFilesystemDirectory GetDirectory(string path)
        {
            return new FilesystemDirectoryLongPath(path);
        }

        public Stream OpenFileForRead(string path)
        {
            return File.OpenRead(path);
        }
    }
}
