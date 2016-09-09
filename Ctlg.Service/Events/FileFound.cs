namespace Ctlg.Service.Events
{
    public class FileFound: IDomainEvent
    {
        public FileFound(string fullPath)
        {
            FullPath = fullPath;
        }

        public string FullPath { get; }
    }
}
