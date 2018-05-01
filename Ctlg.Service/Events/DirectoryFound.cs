namespace Ctlg.Service.Events
{
    public class DirectoryFound: IDomainEvent
    {
        public DirectoryFound(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
