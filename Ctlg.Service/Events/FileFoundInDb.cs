using Ctlg.Data.Model;

namespace Ctlg.Service.Events
{
    public class FileFoundInDb : IDomainEvent
    {
        public FileFoundInDb(File file)
        {
            File = file;
        }

        public File File { get; }
    }
}
