namespace Ctlg.Service.Events
{
    public class DirectoryFound: IDomainEvent
    {
        public DirectoryFound(string fullPath)
        {
            FullPath = fullPath;
        }

        public string FullPath { get; }
    }
}
