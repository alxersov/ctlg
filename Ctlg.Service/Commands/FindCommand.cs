using Ctlg.Service.Utils;

namespace Ctlg.Service.Commands
{
    public class FindCommand: ICommand
    {
        public string Hash { get; set; }

        public void Execute(ICtlgService ctlgService)
        {
            var bytes = FormatBytes.ToByteArray(Hash);
            ctlgService.FindFiles(bytes);
        }
    }
}
