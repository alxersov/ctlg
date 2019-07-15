using Ctlg.Core;
using Ctlg.Service.Utils;

namespace Ctlg.Service.Commands
{
    public class FindCommand: ICommand
    {
        public string HashFunctionName { get; set; }
        public string Hash { get; set; }
        public long? Size { get; set; }
        public string NamePattern { get; set; }

        public FindCommand(ICtlgService ctlgService)
        {
            CtlgService = ctlgService;
        }

        public void Execute()
        {
            Hash hash = null;
            if (HashFunctionName != null && Hash != null)
            {
                var hashAlgorithm = CtlgService.GetHashAlgorithm(HashFunctionName.ToUpperInvariant());
                var bytes = FormatBytes.ToByteArray(Hash);

                hash = new Hash(hashAlgorithm.HashAlgorithmId, bytes);
            }

            CtlgService.FindFiles(hash, Size, NamePattern);
        }

        private ICtlgService CtlgService { get; }
    }
}
