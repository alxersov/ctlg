namespace Ctlg.Service.Commands
{
    public class AddCommand: ICommand
    {
        public string Path { get; set; }
        public string SearchPattern { get; set; }
        public string HashFunctionName { get; set; }

        public void Execute(ICtlgService svc)
        {
            svc.AddDirectory(Path, SearchPattern, HashFunctionName);
        }
    }
}
