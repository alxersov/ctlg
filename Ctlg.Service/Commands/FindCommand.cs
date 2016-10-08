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

        public void Execute(ICtlgService ctlgService)
        {
            Hash hash = null;
            if (HashFunctionName != null && Hash != null)
            {
                var hashAlgorithm = ctlgService.GetHashAlgorithm(HashFunctionName.ToUpperInvariant());
                var bytes = FormatBytes.ToByteArray(Hash);

                hash = new Hash(hashAlgorithm.HashAlgorithmId, bytes);
            }

            ctlgService.FindFiles(hash, Size, NamePattern);
        }
    }
}
