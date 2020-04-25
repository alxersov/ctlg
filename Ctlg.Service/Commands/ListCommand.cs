using Ctlg.Core;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Commands
{
    public class ListCommand: ICommand
    {
        public ListCommand(ICtlgService ctlgService)
        {
            CtlgService = ctlgService;
        }

        public void Execute(Config config)
        {
            CtlgService.ListFiles();
        }

        private ICtlgService CtlgService { get; }
    }
}
