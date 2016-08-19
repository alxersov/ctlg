namespace Ctlg.Service.Commands
{
    public class AddCommand: ICommand
    {
        public string Path { get; set; }
        
        public void Execute(ICtlgService svc)
        {
            svc.AddDirectory(Path);
        }
    }
}
