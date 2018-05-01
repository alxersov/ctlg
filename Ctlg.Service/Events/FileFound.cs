namespace Ctlg.Service.Events
{
    public class FileFound: IDomainEvent
    {
        public FileFound(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
