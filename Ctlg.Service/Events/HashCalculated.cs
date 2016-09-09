namespace Ctlg.Service.Events
{
    public class HashCalculated: IDomainEvent
    {
        public HashCalculated(string fullPath, byte[] hash)
        {
            FullPath = fullPath;
            Hash = hash;
        }

        public string FullPath { get; }
        public byte[] Hash { get; }
    }
}
