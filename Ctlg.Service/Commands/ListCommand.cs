using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Commands
{
    public class ListCommand: ICommand
    {
        public ListCommand(ICtlgService ctlgService)
        {
            CtlgService = ctlgService;
        }

        public void Execute()
        {
            CtlgService.ListFiles();
        }

        private ICtlgService CtlgService { get; }
    }
}
