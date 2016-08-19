namespace Ctlg.Service.Commands
{
    public class ListCommand: ICommand
    {
        public void Execute(ICtlgService ctlgService)
        {
            ctlgService.ListFiles();
        }
    }
}
