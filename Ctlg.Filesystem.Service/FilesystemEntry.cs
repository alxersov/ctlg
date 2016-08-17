using File = Ctlg.Data.Model.File;

namespace Ctlg.Filesystem.Service
{
    public class FilesystemEntry : IFilesystemEntry
    {
        public string FullPath { get; set; }
        public File File { get; set; }
    }
}
