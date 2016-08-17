namespace Ctlg.Service.Commands
{
    public interface ICommand
    {
        void Execute(ICtlgService ctlgService);
    }
}
