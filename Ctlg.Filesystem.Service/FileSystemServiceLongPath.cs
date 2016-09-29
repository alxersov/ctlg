using Stream = System.IO.Stream;
using Pri.LongPath;

namespace Ctlg.Filesystem.Service
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
