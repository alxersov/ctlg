using Ctlg.Data.Model;

namespace Ctlg.Filesystem.Service
{
    public interface IFilesystemEntry
    {
        File File { get; set; }
        string FullPath { get; set; }
    }
}
