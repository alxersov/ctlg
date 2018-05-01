namespace Ctlg.Service.Events
{
    public class ArchiveFound : IDomainEvent
    {
        public ArchiveFound(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
