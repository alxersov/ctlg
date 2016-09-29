namespace Ctlg.Service.Events
{
    public class ArchiveFound : IDomainEvent
    {
        public ArchiveFound(string fullPath)
        {
            FullPath = fullPath;
        }

        public string FullPath { get; }
    }
}
