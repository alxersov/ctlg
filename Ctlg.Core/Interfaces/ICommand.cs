using Ctlg.Core;

namespace Ctlg.Service.Commands
{
    public interface ICommand
    {
        void Execute(Config config);
    }
}
